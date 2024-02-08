namespace api.Repositories;

public class FollowRepository : IFollowRepository
{
    #region Db and vars
    private readonly IMongoClient _client; // used for Session
    private readonly IMongoCollection<Follow> _collection;
    private readonly IMongoCollection<AppUser> _collectionUsers;
    private readonly ILogger<FollowRepository> _logger;

    // constructor - dependency injections
    public FollowRepository(
        IMongoClient client, IMongoDbSettings dbSettings, ILogger<FollowRepository> logger
        )
    {
        _client = client; // used for Session
        var dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<Follow>("follows");
        _collectionUsers = dbName.GetCollection<AppUser>("users");

        _logger = logger;
    }
    #endregion

    public async Task<FolowStatus> AddFollowAsync(string? loggedInUserEmail, string targetMemberEmail, CancellationToken cancellationToken)
    {
        FolowStatus followStatus = new();

        AppUser? followerAppUser = await _collectionUsers.Find<AppUser>(appUser => appUser.Email == loggedInUserEmail).FirstOrDefaultAsync(cancellationToken);
        AppUser? followedAppUser = await _collectionUsers.Find<AppUser>(appUser => appUser.Email == targetMemberEmail).FirstOrDefaultAsync(cancellationToken);

        if (followedAppUser is null)
        {
            followStatus.IsTargetMemberEmailWrong = true;
            return followStatus;
        }

        bool IsAlreadyFollowed = await _collection.Find<Follow>(follow =>
        follow.FollowerId == followerAppUser.Id && follow.FollowedId == followedAppUser.Id).AnyAsync(cancellationToken);

        if (IsAlreadyFollowed)
        {
            followStatus.IsAlreadyFollowed = true;
            return followStatus;
        }


        Follow? follow = Mappers.ConvertAppUsertoFollow(followerAppUser.Id, followedAppUser.Id);

        if (follow is not null)
        {
            bool isSuccess = await SaveInDbWithSessionAsync(follow, followedAppUser.Id, cancellationToken);

            followStatus.IsSuccess = isSuccess;
        }


        return followStatus; // Faild
    }

    public async Task<IEnumerable<MemberDto>> GetFollowMembersAsync(string? loggedInUserEmail, string predicate, CancellationToken cancellationToken)
    {
        // First get appUser Id then look for follows by Id instead of Email to improve performance. Searching by ObjectId is more secure and performant than string.
        ObjectId? loggedInUserId = await _collectionUsers.AsQueryable<AppUser>()
            .Where(appUser => appUser.Email == loggedInUserEmail)
            .Select(appUser => appUser.Id)
            .FirstOrDefaultAsync(cancellationToken);

        IEnumerable<AppUser> appUsers = await GetAllFollowsFromDBAsync(loggedInUserId, predicate, cancellationToken);

        return Mappers.ConvertAppUsersToMemberDtos(appUsers);
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
    private async Task<IEnumerable<AppUser>> GetAllFollowsFromDBAsync(ObjectId? loggedInUserId, string predicate, CancellationToken cancellationToken)
    {
        if (predicate.Equals("followings"))
        {
            return await _collection.AsQueryable<Follow>()
                        .Where(follow => follow.FollowerId == loggedInUserId) // filter by Lisa's id
                        .Join(_collectionUsers.AsQueryable<AppUser>(), // get follows list which are followed by the followerId/loggedInUserId
                            follow => follow.FollowedId, // map each followedId user with their AppUser Id bellow
                            appUser => appUser.Id,
                            (follow, appUser) => appUser).ToListAsync(cancellationToken); // project the AppUser
        }

        if (predicate.Equals("followers"))
        {
            return await _collection.AsQueryable<Follow>()
                .Where(follow => follow.FollowedId == loggedInUserId)
                .Join(_collectionUsers.AsQueryable<AppUser>(),
                    follow => follow.FollowerId,
                    appUser => appUser.Id,
                    (follow, appUser) => appUser).ToListAsync(cancellationToken);
        }

        return [];
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
