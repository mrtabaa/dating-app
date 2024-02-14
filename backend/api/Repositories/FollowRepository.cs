
namespace api.Repositories;

public class FollowRepository : IFollowRepository
{
    #region Db and vars
    private readonly IMongoClient _client; // used for Session
    private readonly IMongoCollection<Follow> _collection;
    private readonly IMongoCollection<AppUser> _collectionUsers;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<FollowRepository> _logger;

    // constructor - dependency injections
    public FollowRepository(
        IMongoClient client, IMongoDbSettings dbSettings, IUserRepository userRepository, ILogger<FollowRepository> logger
        )
    {
        _client = client; // used for Session
        var dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<Follow>(AppVariablesExtensions.collectionFollows);
        _collectionUsers = dbName.GetCollection<AppUser>(AppVariablesExtensions.collectionUsers);

        _userRepository = userRepository;

        _logger = logger;
    }
    #endregion

    public async Task<FolowStatus> AddFollowAsync(string? loggedInUserEmail, string followedMemberEmail, CancellationToken cancellationToken)
    {
        FolowStatus followStatus = new();

        if (string.IsNullOrEmpty(loggedInUserEmail))
        {
            followStatus.IsLoggedInUserEmailInvalid = true;
            return followStatus;
        }

        ObjectId? loggedInUserId = await _userRepository.GetIdByEmailAsync(loggedInUserEmail, cancellationToken);
        ObjectId? followedUserId = await _userRepository.GetIdByEmailAsync(followedMemberEmail, cancellationToken);

        if (followedUserId.Equals(ObjectId.Empty))
        {
            followStatus.IsTargetMemberEmailWrong = true;
            return followStatus;
        }

        bool IsAlreadyFollowed = await _collection.Find<Follow>(follow =>
        follow.FollowerId == loggedInUserId && follow.FollowedMemberId == followedUserId).AnyAsync(cancellationToken);

        if (IsAlreadyFollowed)
        {
            followStatus.IsAlreadyFollowed = true;
            return followStatus;
        }

        Follow? follow = Mappers.ConvertAppUsertoFollow(loggedInUserId, followedUserId);

        if (follow is not null)
        {
            bool isSuccess = await SaveInDbWithSessionAsync(follow, followedUserId, cancellationToken);

            followStatus.IsSuccess = isSuccess;
        }


        return followStatus; // Faild
    }

    public async Task<PagedList<AppUser>> GetFollowMembersAsync(string loggedInUserEmail, FollowParams followParams, CancellationToken cancellationToken)
    {
        // First get appUser Id then look for follows by Id instead of Email to improve performance. Searching by ObjectId is more secure and performant than string.
        ObjectId? loggedInUserId = await _userRepository.GetIdByEmailAsync(loggedInUserEmail, cancellationToken);

        followParams.LoggedInUserId = loggedInUserId;

        return await GetAllFollowsFromDBAsync(followParams, cancellationToken);
    }

    /// <summary>
    /// InsertOneAsync the 'follow'.
    /// Increase Member's FollowdCount by 1.
    /// Use MongoDb Transaction/Session to rollback if Update Member fails.
    /// </summary>
    /// <param name="follow"></param>
    /// <param name="followedId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>bool isSuccess</returns>
    private async Task<bool> SaveInDbWithSessionAsync(Follow follow, ObjectId? followedId, CancellationToken cancellationToken)
    {
        //// Session is NOT supported in MongoDb Standalone servers!
        // Create a session object that is used when leveraging transactions
        using var session = await _client.StartSessionAsync(null, cancellationToken);

        // Begin transaction
        session.StartTransaction();

        try
        {
            // added session to this part so if UpdateFollowedByCount failed, undo the InsertOnce.
            await _collection.InsertOneAsync(session, follow, null, cancellationToken);

            await UpdateFollowedByCount(session, followedId, cancellationToken);

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
        if (followParams.Predicate.Equals("followings"))
        {
            var query = _collection.AsQueryable<Follow>()
                        .Where(follow => follow.FollowerId == followParams.LoggedInUserId) // filter by Lisa's id
                        .Join(_collectionUsers.AsQueryable<AppUser>(), // get follows list which are followed by the followerId/loggedInUserId
                            follow => follow.FollowedMemberId, // map each followedId user with their AppUser Id bellow
                            appUser => appUser.Id,
                            (follow, appUser) => appUser); // project the AppUser

            return await PagedList<AppUser>.CreatePagedListAsync(query, followParams.PageNumber, followParams.PageSize, cancellationToken);
        }
        else //(followParams.Predicate.Equals("followers"))
        {
            var query = _collection.AsQueryable<Follow>()
                .Where(follow => follow.FollowedMemberId == followParams.LoggedInUserId)
                .Join(_collectionUsers.AsQueryable<AppUser>(),
                    follow => follow.FollowerId,
                    appUser => appUser.Id,
                    (follow, appUser) => appUser);

            return await PagedList<AppUser>.CreatePagedListAsync(query, followParams.PageNumber, followParams.PageSize, cancellationToken);
        }
    }

    /// <summary>
    /// Increament FollowersCount of the member who was followed. (TargetMember)
    /// This is part of a MondoDb session. MongoDb will undo Insert and Update if any fails. So we don't need to verify the completion of the db process.
    /// </summary>
    /// <param name="followedId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task UpdateFollowedByCount(IClientSessionHandle session, ObjectId? followedId, CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser> updateFollowedByCount = Builders<AppUser>.Update
        .Inc(appUser => appUser.FollowersCount, 1); // Increament by 1 for each follow

        await _collectionUsers.UpdateOneAsync<AppUser>(session, appUser =>
                appUser.Id == followedId, updateFollowedByCount, null, cancellationToken);
    }
}
