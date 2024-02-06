namespace api.Repositories;

public class LikeRepository : ILikeRepository
{
    #region Db and Token Settings
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
        LikeStatus likeStatus = new();

        AppUser? loggedInAppUser = await _collectionUsers.Find<AppUser>(appUser => appUser.Email == loggedInUserEmail).FirstOrDefaultAsync(cancellationToken);
        AppUser? targetMemberAppUser = await _collectionUsers.Find<AppUser>(appUser => appUser.Email == targetMemberEmail).FirstOrDefaultAsync(cancellationToken);

        if (!(loggedInAppUser.Id is null || targetMemberAppUser.Id is null))
        {
            bool IsAlreadyLiked = await _collection.Find<Like>(like =>
            like.LoggedInUserId == loggedInAppUser.Id && like.TargetMemberId == targetMemberAppUser.Id).AnyAsync(cancellationToken);

            if (IsAlreadyLiked)
            {
                likeStatus.IsAlreadyLiked = true;
                return likeStatus;
            }

            // likes.Add()
            Like? like = Mappers.ConvertAppUsertoLike(loggedInAppUser.Id, targetMemberAppUser.Id);

            if (like is not null)
            {
                bool isSuccess = await SaveInDbWithSession(like, targetMemberAppUser.Id, cancellationToken);

                likeStatus.IsSuccess = isSuccess;
            }

        }

        return likeStatus; // Faild
    }

    public async Task<List<MemberDto>> GetLikedMembersAsync(string? loggedInUserEmail, string predicate, CancellationToken cancellationToken)
    {
        // First get appUser Id then look for likes by Id instead of Email to improve performance. Searching by ObjectId is more secure and performant than string.
        // TODO replace with ObjectId
        string? loggedInUserId = await _collectionUsers.AsQueryable<AppUser>()
            .Where(appUser => appUser.Email == loggedInUserEmail)
            .Select(appUser => appUser.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (predicate.Equals("liked"))
        {
            // IEnumerable<Like> likes = await _collection.Find<Like>(like => like.LoggedInUser.Id == loggedInUserId)
            //     .ToListAsync(cancellationToken);

            // return ConvertLikesToLikeDtos(likes);
        }

        if (predicate.Equals("liked-by"))
        {
            // IEnumerable<Like> likes = await _collection.Find<Like>(like => like.TargetMember.Id == loggedInUserId)
            //     .ToListAsync(cancellationToken);

            // return ConvertLikesToLikeDtos(likes);
        }

        return [];
    }

    /// <summary>
    /// InsertOneAsync the 'like'.
    /// Increase Member's LikedCount by 1.
    /// Use MongoDb Transaction/Session to rollback if Update Member fails.
    /// </summary>
    /// <param name="like"></param>
    /// <param name="targetMemberId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>bool isSuccess</returns>
    private async Task<bool> SaveInDbWithSession(Like like, string targetMemberId, CancellationToken cancellationToken)
    {
        //// Session is NOT supported in MongoDb Standalone servers!
        // Create a session object that is used when leveraging transactions
        using var session = await _client.StartSessionAsync(null, cancellationToken);

        // Begin transaction
        session.StartTransaction();

        try
        {
            // TODO add session to this part so if UpdateLikedByCount failed, undo the InsertOnce.
            await _collection.InsertOneAsync(session, like, null, cancellationToken);

            await UpdateLikedByCount(session, targetMemberId, cancellationToken);

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
    /// Increament Liked-byCount of the member who was liked. (TargetMember)
    /// This is part of a MondoDb session. MongoDb will undo Insert and Update if any fails. So we don't need to verify the completion of the db process.
    /// </summary>
    /// <param name="targetMemberId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task UpdateLikedByCount(IClientSessionHandle session, string? targetMemberId, CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser> updateLikedByCount = Builders<AppUser>.Update
        .Inc(appUser => appUser.Liked_byCount, 1); // Increament by 1 for each like

        await _collectionUsers.UpdateOneAsync<AppUser>(session, appUser =>
                appUser.Id == targetMemberId, updateLikedByCount, null, cancellationToken);
    }
}
