namespace api.Repositories;

public class FollowRepository : IFollowRepository
{
    /// <summary>
    ///     Find only members which the loggedInUser is following.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="followParams"></param>
    /// <param name="cancellationToken"></param>
    /// <returns AppUser="?. Return null if userId is invalid">PagedList</returns>
    public async Task<PagedList<AppUser>?> GetFollowMembersAsync(ObjectId userId, FollowParams followParams, CancellationToken cancellationToken)
    {
        followParams.UserId = userId;

        return await GetAllFollowsFromDbAsync(followParams, cancellationToken);
    }

    /// <summary>
    ///     /// Gets a member UserName and follows the member. A Follow doc is added to the db "follows" collection.
    ///     The UpdateFollowingsCount() UpdateFollowersCount() are called to increment 1 in their AppUsers.
    ///     MongoDb Session/Transaction is used.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="followedMemberUserName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>FollowStatus</returns>
    public async Task<FollowStatus> AddFollowAsync(ObjectId userId, string followedMemberUserName, CancellationToken cancellationToken)
    {
        FollowStatus followStatus = new();

        ObjectId? followedMemberId = await _userRepository.GetIdByUserNameAsync(followedMemberUserName, cancellationToken);

        if (followedMemberId is null)
        {
            followStatus.IsTargetMemberNotFound = true;
            return followStatus;
        }

        if (userId == followedMemberId)
        {
            followStatus.IsFollowingThemself = true;
            return followStatus;
        }

        bool isAlreadyFollowed = await _collection.Find(follow =>
                follow.FollowerId == userId && follow.FollowedMemberId == followedMemberId)
            .AnyAsync(cancellationToken);

        if (isAlreadyFollowed)
        {
            followStatus.IsAlreadyFollowed = true;
            return followStatus;
        }

        Follow follow = Mappers.ConvertAppUserToFollow(userId, followedMemberId.Value);

        followStatus.IsSuccess = await SaveInDbWithSessionAsync(userId, followedMemberId.Value, FollowAction.IsAdded, cancellationToken, follow);

        return followStatus; // Failed for any other reason
    }

    /// <summary>
    ///     Gets the followed member UserName and unfollows the member. A Follow doc is removed from the db "follows"
    ///     collection.
    ///     The UpdateFollowingsCount() UpdateFollowersCount() are called to decrement 1 from their AppUsers.
    ///     MongoDb Session/Transaction is used.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="followedMemberUserName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>true: success. false: fail</returns>
    public async Task<FollowStatus> RemoveFollowAsync(ObjectId userId, string followedMemberUserName, CancellationToken cancellationToken)
    {
        FollowStatus followStatus = new();

        ObjectId? followedMemberId = await _userRepository.GetIdByUserNameAsync(followedMemberUserName, cancellationToken);

        if (followedMemberId is null)
        {
            followStatus.IsTargetMemberNotFound = true;
            return followStatus;
        }

        followStatus.IsSuccess = await SaveInDbWithSessionAsync(userId, followedMemberId.Value, FollowAction.IsRemoved, cancellationToken);

        return followStatus;
    }

    /// <summary>
    ///     Check if the loggedIn user is following the specific member/appUser or not.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="appUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>true: following; false: not following</returns>
    public async Task<bool> CheckIsFollowing(ObjectId userId, AppUser appUser, CancellationToken cancellationToken) =>
        await _collection.AsQueryable()
            .Where(follow => follow.FollowerId == userId && appUser.Id == follow.FollowedMemberId)
            .AnyAsync(cancellationToken);

    /// <summary>
    ///     InsertOneAsync the 'follow'.
    ///     Increase Member's FollowedCount by 1.
    ///     Use MongoDb Transaction/Session to rollback if Update Member fails.
    /// </summary>
    /// <param name="follow"></param>
    /// <param name="userId"></param>
    /// <param name="followedId"></param>
    /// <param name="followAddOrRemove"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>bool isSuccess</returns>
    private async Task<bool> SaveInDbWithSessionAsync(
        ObjectId userId,
        ObjectId followedId,
        FollowAction followAddOrRemove,
        CancellationToken cancellationToken,
        Follow? follow = null
    )
    {
        //// Session is NOT supported in MongoDb Standalone servers!
        // Create a session object that is used when leveraging transactions
        using IClientSessionHandle? session = await _client.StartSessionAsync(null, cancellationToken);

        // Begin transaction
        session.StartTransaction();

        try
        {
            if (follow is not null && FollowAction.IsAdded == followAddOrRemove) // Add follow
                // added session to this part so if UpdateFollowedByCount failed, undo the InsertOnce.
                await _collection.InsertOneAsync(session, follow, null, cancellationToken);
            else // if (FollowAddOrRemove.IsRemoved == followAddOrRemove) // Remove follow
            {
                FilterDefinition<Follow>? filter = Builders<Follow>.Filter.Where(fol => fol.FollowerId == userId && fol.FollowedMemberId == followedId);

                // follow doc doesn't exist. May be already deleted. 
                if (!await _collection.Find(filter).AnyAsync(cancellationToken))
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
    ///     Get loggedInUserId and return all follower or followed appUsers from DB
    /// </summary>
    /// <param name="followParams"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>appUsers</returns>
    private async Task<PagedList<AppUser>?> GetAllFollowsFromDbAsync(FollowParams followParams, CancellationToken cancellationToken)
    {
        if (followParams.Predicate == FollowPredicate.Followings)
        {
            IMongoQueryable<AppUser>? query = _collection.AsQueryable()
                .Where(follow => follow.FollowerId == followParams.UserId) // filter by Lisa's id
                .Join(_collectionUsers.AsQueryable<AppUser>(), // get follows list which are followed by the followerId/loggedInUserId
                    follow => follow.FollowedMemberId, // map each followedId user with their AppUser Id bellow
                    appUser => appUser.Id,
                    (follow, appUser) => appUser); // project the AppUser

            PagedList<AppUser>? appUsers = await PagedList<AppUser>.CreatePagedListAsync(query, followParams.PageNumber, followParams.PageSize, cancellationToken);

            appUsers = GetAppUsersWithBlobPhotos(appUsers);

            return appUsers;
        }
        else // (followParams.Predicate == FollowPredicate.Followers)
        {
            IMongoQueryable<AppUser>? query = _collection.AsQueryable<Follow>()
                .Where(follow => follow.FollowedMemberId == followParams.UserId)
                .Join(_collectionUsers.AsQueryable<AppUser>(),
                    follow => follow.FollowerId,
                    appUser => appUser.Id,
                    (follow, appUser) => appUser);

            PagedList<AppUser>? appUsers = await PagedList<AppUser>
                .CreatePagedListAsync(query, followParams.PageNumber, followParams.PageSize, cancellationToken);

            appUsers = GetAppUsersWithBlobPhotos(appUsers);

            return appUsers;
        }
    }

    /// <summary>
    ///     Increment FollowingsCount of the member who followed another member. (Follower member)
    ///     This is part of a MongoDb session. MongoDb will undo Insert and Update if any fails. So we don't need to verify the
    ///     completion of the db process.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="followerId"></param>
    /// <param name="followAddOrRemove"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task UpdateFollowingsCount(IClientSessionHandle session, ObjectId followerId, FollowAction followAddOrRemove, CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser> updateFollowedByCount;

        if (followAddOrRemove == FollowAction.IsAdded)
        {
            updateFollowedByCount = Builders<AppUser>.Update
                .Inc(appUser => appUser.FollowingsCount, 1); // Increment by 1 for each follow
        }
        else
        {
            updateFollowedByCount = Builders<AppUser>.Update
                .Inc(appUser => appUser.FollowingsCount, -1); // Decrement by 1 for each unfollow
        }

        await _collectionUsers.UpdateOneAsync(session, appUser =>
            appUser.Id == followerId, updateFollowedByCount, null, cancellationToken);
    }

    /// <summary>
    ///     Increment FollowersCount of the member who was followed. (TargetMember)
    ///     This is part of a MongoDb session. MongoDb will undo Insert and Update if any fails. So we don't need to verify the
    ///     completion of the db process.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="followedId"></param>
    /// <param name="followAddOrRemove"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task UpdateFollowersCount(IClientSessionHandle session, ObjectId followedId, FollowAction followAddOrRemove, CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser> updateFollowerCount;

        if (followAddOrRemove == FollowAction.IsAdded)
        {
            updateFollowerCount = Builders<AppUser>.Update
                .Inc(appUser => appUser.FollowersCount, 1); // Increment by 1 for each follow
        }
        else
        {
            updateFollowerCount = Builders<AppUser>.Update
                .Inc(appUser => appUser.FollowersCount, -1); // Decrement by 1 for each unfollow
        }

        await _collectionUsers.UpdateOneAsync(session, appUser =>
            appUser.Id == followedId, updateFollowerCount, null, cancellationToken);
    }

    /// Convert all members' appUser.Photos to BlobLinkFormat
    private PagedList<AppUser>? GetAppUsersWithBlobPhotos(PagedList<AppUser> appUsers)
    {
        for (var i = 0; i < appUsers.Count; i++)
        {
            if (appUsers[i].Photos.Count <= 0) continue; // continue only if appUser has a photo

            AppUser? appUser = ConvertAppUserPhotosToBlobPhotos(appUsers[i]);

            if (appUser is null) return null;

            appUsers[i] = appUser;
        }

        return appUsers;
    }

    private AppUser? ConvertAppUserPhotosToBlobPhotos(AppUser appUser)
    {
        List<Photo>? blobConvertedPhotos = _photoService.ConvertAllPhotosToBlobLinkWithSas(appUser.Photos)?.ToList();
        if (blobConvertedPhotos is null)
            return null;

        appUser.Photos = blobConvertedPhotos;

        return appUser;
    }

    #region Db and vars

    private readonly IMongoClient _client; // used for Session
    private readonly IMongoCollection<Follow> _collection;
    private readonly IMongoCollection<AppUser> _collectionUsers;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<FollowRepository> _logger;
    private readonly IPhotoService _photoService;

    // constructor - dependency injections
    public FollowRepository(
        IMongoClient client, IMyMongoDbSettings dbSettings, IUserRepository userRepository,
        IPhotoService photoService, ILogger<FollowRepository> logger
    )
    {
        _client = client; // used for Session
        IMongoDatabase dbName = client.GetDatabase(dbSettings.DatabaseName) ?? throw new ArgumentNullException(nameof(dbName));
        _collection = dbName.GetCollection<Follow>(AppVariablesExtensions.CollectionFollows);
        _collectionUsers = dbName.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);

        _userRepository = userRepository;
        _photoService = photoService;

        _logger = logger;
    }

    #endregion
}