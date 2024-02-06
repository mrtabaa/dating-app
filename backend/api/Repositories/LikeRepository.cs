namespace api.Repositories;

public class LikeRepository : ILikeRepository
{
    #region Db and vars
    private readonly IMongoClient _client; // used for Session
    private readonly IMongoCollection<Like> _collection;
    private readonly IMongoCollection<AppUser> _collectionUsers;
    private readonly ILogger<LikeRepository> _logger;

    // constructor - dependency injections
    public LikeRepository(
        IMongoClient client, IMongoDbSettings dbSettings, ILogger<LikeRepository> logger
        )
    {
        _client = client; // used for Session
        var dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<Like>("likes");
        _collectionUsers = dbName.GetCollection<AppUser>("users");

        _logger = logger;
    }
    #endregion

    public async Task<LikeStatus> AddLikeAsync(string? loggedInUserEmail, string targetMemberEmail, CancellationToken cancellationToken)
    {
        //TODO Prevent from entering wrong email

        LikeStatus likeStatus = new();

        AppUser? likerAppUser = await _collectionUsers.Find<AppUser>(appUser => appUser.Email == loggedInUserEmail).FirstOrDefaultAsync(cancellationToken);
        AppUser? likedAppUser = await _collectionUsers.Find<AppUser>(appUser => appUser.Email == targetMemberEmail).FirstOrDefaultAsync(cancellationToken);

        bool IsAlreadyLiked = await _collection.Find<Like>(like =>
        like.LikerId == likerAppUser.Id && like.LikedId == likedAppUser.Id).AnyAsync(cancellationToken);

        if (IsAlreadyLiked)
        {
            likeStatus.IsAlreadyLiked = true;
            return likeStatus;
        }


        Like? like = Mappers.ConvertAppUsertoLike(likerAppUser.Id, likedAppUser.Id);

        if (like is not null)
        {
            bool isSuccess = await SaveInDbWithSessionAsync(like, likedAppUser.Id, cancellationToken);

            likeStatus.IsSuccess = isSuccess;
        }


        return likeStatus; // Faild
    }

    public async Task<IEnumerable<MemberDto>> GetLikeMembersAsync(string? loggedInUserEmail, string predicate, CancellationToken cancellationToken)
    {
        // First get appUser Id then look for likes by Id instead of Email to improve performance. Searching by ObjectId is more secure and performant than string.
        ObjectId? loggedInUserId = await _collectionUsers.AsQueryable<AppUser>()
            .Where(appUser => appUser.Email == loggedInUserEmail)
            .Select(appUser => appUser.Id)
            .FirstOrDefaultAsync(cancellationToken);

        IEnumerable<AppUser> appUsers = await GetAllLikesFromDBAsync(loggedInUserId, predicate, cancellationToken);

        return Mappers.ConvertAppUsersToMemberDtos(appUsers);
    }

    /// <summary>
    /// InsertOneAsync the 'like'.
    /// Increase Member's LikedCount by 1.
    /// Use MongoDb Transaction/Session to rollback if Update Member fails.
    /// </summary>
    /// <param name="like"></param>
    /// <param name="likedId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>bool isSuccess</returns>
    private async Task<bool> SaveInDbWithSessionAsync(Like like, ObjectId? likedId, CancellationToken cancellationToken)
    {
        //// Session is NOT supported in MongoDb Standalone servers!
        // Create a session object that is used when leveraging transactions
        using var session = await _client.StartSessionAsync(null, cancellationToken);

        // Begin transaction
        session.StartTransaction();

        try
        {
            // added session to this part so if UpdateLikedByCount failed, undo the InsertOnce.
            await _collection.InsertOneAsync(session, like, null, cancellationToken);

            await UpdateLikedByCount(session, likedId, cancellationToken);

            await session.CommitTransactionAsync(cancellationToken);

            return true;
        }
        catch (System.Exception ex)
        {
            await session.AbortTransactionAsync(cancellationToken);

            _logger.LogError("Like failed. Error writing to MongoDB" + ex.Message);

            return false;
        }
    }

    /// <summary>
    /// Get loggedInUserId and return all liker or liked appUsers from DB
    /// </summary>
    /// <param name="loggedInUserId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>appUsers</returns>
    private async Task<IEnumerable<AppUser>> GetAllLikesFromDBAsync(ObjectId? loggedInUserId, string predicate, CancellationToken cancellationToken)
    {
        if (predicate.Equals("liked"))
        {
            return await _collection.AsQueryable<Like>()
                        .Where(like => like.LikerId == loggedInUserId) // filter by Lisa's id
                        .Join(_collectionUsers.AsQueryable<AppUser>(), // get likes list which are liked by the likerId/loggedInUserId
                            like => like.LikedId, // map each likedId user with their AppUser Id bellow
                            appUser => appUser.Id,
                            (like, appUser) => appUser).ToListAsync(cancellationToken); // project the AppUser
        }

        if (predicate.Equals("liked-by"))
        {
            return await _collection.AsQueryable<Like>()
                .Where(like => like.LikedId == loggedInUserId)
                .Join(_collectionUsers.AsQueryable<AppUser>(),
                    like => like.LikerId,
                    appUser => appUser.Id,
                    (like, appUser) => appUser).ToListAsync(cancellationToken);
        }

        return [];
    }

    /// <summary>
    /// Increament Liked-byCount of the member who was liked. (TargetMember)
    /// This is part of a MondoDb session. MongoDb will undo Insert and Update if any fails. So we don't need to verify the completion of the db process.
    /// </summary>
    /// <param name="likedId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task UpdateLikedByCount(IClientSessionHandle session, ObjectId? likedId, CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser> updateLikedByCount = Builders<AppUser>.Update
        .Inc(appUser => appUser.Liked_byCount, 1); // Increament by 1 for each like

        await _collectionUsers.UpdateOneAsync<AppUser>(session, appUser =>
                appUser.Id == likedId, updateLikedByCount, null, cancellationToken);
    }
}
