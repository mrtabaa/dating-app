
namespace api.Repositories;

public class FollowRepository : IFollowRepository
{
    #region Db and vars
    private readonly IMongoClient _client; // used for Session
    private readonly IMongoCollection<Follow> _collection;
    private readonly IMongoCollection<AppUser> _collectionUsers;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<FollowRepository> _logger;

    // constructor - dependency injections
    public FollowRepository(
        IMongoClient client, IMyMongoDbSettings dbSettings, IUserRepository userRepository, ITokenService tokenService, ILogger<FollowRepository> logger
        )
    {
        _client = client; // used for Session
        var dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<Follow>(AppVariablesExtensions.collectionFollows);
        _collectionUsers = dbName.GetCollection<AppUser>(AppVariablesExtensions.collectionUsers);

        _tokenService = tokenService;
        _userRepository = userRepository;

        _logger = logger;
    }
    #endregion
    /// <summary>
    /// Find only members which the loggedInUser is following. 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="followParams"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>PagedList<AppUser>?. Return null if userId is invalid</returns>
    public async Task<PagedList<AppUser>?> GetFollowMembersAsync(ObjectId userId, FollowParams followParams, CancellationToken cancellationToken)
    {
        followParams.UserId = userId;

        return await GetAllFollowsFromDBAsync(followParams, cancellationToken);
    }

    /// <summary>
    /// /// Gets a member UserName and follows the member. A Follow doc is added to the db "follows" collection.
    /// The UpdateFollowingsCount() UpdateFollowersCount() are called to increment 1 in their AppUsers.
    /// MongoDb Session/Transaction is used. 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="followedMemberUserName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>FollowStatus</returns>
    public async Task<FollowStatus> AddFollowAsync(ObjectId userId, string followedMemberUserName, CancellationToken cancellationToken)
    {
        FollowStatus followStatus = new();

        AppUser? followedMember = await _userRepository.GetByUserNameAsync(followedMemberUserName, cancellationToken);

        if (followedMember is null)
        {
            followStatus.IsTargetMemberNotFound = true;
            return followStatus;
        }

        if (userId == followedMember.Id)
        {
            followStatus.IsFollowingThemself = true;
            return followStatus;
        }

        bool IsAlreadyFollowed = await _collection.Find<Follow>(follow =>
            follow.FollowerId == userId && follow.FollowedMemberId == followedMember.Id)
            .AnyAsync(cancellationToken);

        // set knownAs to show in controller's messages
        followStatus.KnownAs = followedMember.KnownAs;

        if (IsAlreadyFollowed)
        {
            followStatus.IsAlreadyFollowed = true;
            return followStatus;
        }

        Follow? follow = Mappers.ConvertAppUsertoFollow(userId, followedMember);

        if (follow is not null)
        {
            bool isSuccess = await SaveInDbWithSessionAsync(userId, followedMember.Id, FollowAddOrRemove.IsAdded, cancellationToken, follow);

            followStatus.IsSuccess = isSuccess;
        }

        return followStatus; // Faild for any other reason
    }

    /// <summary>
    /// Gets the followed member UserName and unfollows the member. A Follow doc is removed from the db "follows" collection.
    /// The UpdateFollowingsCount() UpdateFollowersCount() are called to decrement 1 from their AppUsers. 
    /// MongoDb Session/Transaction is used. 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="followedMemberUserName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>true: sucess. false: fail</returns>
    public async Task<FollowStatus> RemoveFollowAsync(ObjectId userId, string followedMemberUserName, CancellationToken cancellationToken)
    {
        FollowStatus followStatus = new();

        AppUser? followedMember = await _userRepository.GetByUserNameAsync(followedMemberUserName, cancellationToken);

        if (followedMember is null)
        {
            followStatus.IsTargetMemberNotFound = true;
            return followStatus;
        }

        // set knownAs to show in controller's messages
        followStatus.KnownAs = followedMember.KnownAs;

        bool isSuccess = await SaveInDbWithSessionAsync(userId, followedMember.Id, FollowAddOrRemove.IsRemoved, cancellationToken);

        if (isSuccess)
            followStatus.IsSuccess = true;

        return followStatus;
    }

    /// <summary>
    /// Check if the loggedIn user is following the specific member/appUser or not. 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="appUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>true: following; false: not following</returns>
    public async Task<bool> CheckIsFollowing(ObjectId userId, AppUser appUser, CancellationToken cancellationToken) =>
        await _collection.AsQueryable<Follow>()
            .Where(follow => follow.FollowerId == userId && appUser.Id == follow.FollowedMemberId)
            .AnyAsync(cancellationToken);

    /// <summary>
    /// InsertOneAsync the 'follow'.
    /// Increase Member's FollowdCount by 1.
    /// Use MongoDb Transaction/Session to rollback if Update Member fails.
    /// </summary>
    /// <param name="follow"></param>
    /// <param name="followedId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>bool isSuccess</returns>
    private async Task<bool> SaveInDbWithSessionAsync(
        ObjectId userId,
        ObjectId followedId,
        FollowAddOrRemove followAddOrRemove,
        CancellationToken cancellationToken,
        Follow? follow = null
    )
    {
        //// Session is NOT supported in MongoDb Standalone servers!
        // Create a session object that is used when leveraging transactions
        using var session = await _client.StartSessionAsync(null, cancellationToken);

        // Begin transaction
        session.StartTransaction();

        try
        {
            if (follow is not null && FollowAddOrRemove.IsAdded == followAddOrRemove) // Add follow
                // added session to this part so if UpdateFollowedByCount failed, undo the InsertOnce.
                await _collection.InsertOneAsync(session, follow, null, cancellationToken);
            else // if (FollowAddOrRemove.IsRemoved == followAddOrRemove) // Remove follow
            {

                var filter = Builders<Follow>.Filter.Where(follow => follow.FollowerId == userId && follow.FollowedMemberId == followedId);

                // follow doc doesn't exists. May be already deleted. 
                if (!await _collection.Find<Follow>(filter).AnyAsync(cancellationToken))
                    return false;

                await _collection.DeleteOneAsync(session, filter, null, cancellationToken);
            }

            await UpdateFollowingsCount(session, userId, followAddOrRemove, cancellationToken);

            await UpdateFollowersCount(session, followedId, followAddOrRemove, cancellationToken);

            await session.CommitTransactionAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync(cancellationToken);

            _logger.LogError("Follow failed. Error writing to MongoDB" + ex.Message);

            return false;
        }
    }

    /// <summary>
    /// Get loggedInUserId and return all follower or followed appUsers from DB
    /// </summary>
    /// <param name="loggedInUserId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>appUsers</returns>
    private async Task<PagedList<AppUser>> GetAllFollowsFromDBAsync(FollowParams followParams, CancellationToken cancellationToken)
    {
        if (followParams.Predicate == FollowPredicate.Followings)
        {
            var query = _collection.AsQueryable<Follow>()
                        .Where(follow => follow.FollowerId == followParams.UserId) // filter by Lisa's id
                        .Join(_collectionUsers.AsQueryable<AppUser>(), // get follows list which are followed by the followerId/loggedInUserId
                            follow => follow.FollowedMemberId, // map each followedId user with their AppUser Id bellow
                            appUser => appUser.Id,
                            (follow, appUser) => appUser); // project the AppUser

            return await PagedList<AppUser>.CreatePagedListAsync(query, followParams.PageNumber, followParams.PageSize, cancellationToken);
        }
        else // (followParams.Predicate == FollowPredicate.Followers)
        {
            var query = _collection.AsQueryable<Follow>()
                .Where(follow => follow.FollowedMemberId == followParams.UserId)
                .Join(_collectionUsers.AsQueryable<AppUser>(),
                    follow => follow.FollowerId,
                    appUser => appUser.Id,
                    (follow, appUser) => appUser);

            return await PagedList<AppUser>.CreatePagedListAsync(query, followParams.PageNumber, followParams.PageSize, cancellationToken);
        }
    }

    /// <summary>
    /// Increament FollowingsCount of the member who followed another member. (Follower member)
    /// This is part of a MondoDb session. MongoDb will undo Insert and Update if any fails. So we don't need to verify the completion of the db process.
    /// </summary>
    /// <param name="followerId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task UpdateFollowingsCount(IClientSessionHandle session, ObjectId followerId, FollowAddOrRemove followAddOrRemove, CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser> updateFollowedByCount;

        if (followAddOrRemove == FollowAddOrRemove.IsAdded)
        {
            updateFollowedByCount = Builders<AppUser>.Update
                .Inc(appUser => appUser.FollowingsCount, 1); // Increament by 1 for each follow
        }
        else
        {
            updateFollowedByCount = Builders<AppUser>.Update
                .Inc(appUser => appUser.FollowingsCount, -1); // Decreament by 1 for each unfollow
        }

        await _collectionUsers.UpdateOneAsync<AppUser>(session, appUser =>
                appUser.Id == followerId, updateFollowedByCount, null, cancellationToken);
    }

    /// <summary>
    /// Increament FollowersCount of the member who was followed. (TargetMember)
    /// This is part of a MondoDb session. MongoDb will undo Insert and Update if any fails. So we don't need to verify the completion of the db process.
    /// </summary>
    /// <param name="followedId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task UpdateFollowersCount(IClientSessionHandle session, ObjectId followedId, FollowAddOrRemove followAddOrRemove, CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser> updateFollowerCount;

        if (followAddOrRemove == FollowAddOrRemove.IsAdded)
        {
            updateFollowerCount = Builders<AppUser>.Update
                .Inc(appUser => appUser.FollowersCount, 1); // Increament by 1 for each follow
        }
        else
        {
            updateFollowerCount = Builders<AppUser>.Update
                .Inc(appUser => appUser.FollowersCount, -1); // Decreament by 1 for each unfollow
        }

        await _collectionUsers.UpdateOneAsync<AppUser>(session, appUser =>
                appUser.Id == followedId, updateFollowerCount, null, cancellationToken);
    }
}
