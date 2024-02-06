
namespace api.Repositories;

public class LikeRepository : ILikeRepository
{
    #region Db and Token Settings
    private readonly IMongoClient _client; // used for Session
    private readonly IMongoCollection<Like> _collection;
    private readonly IMongoCollection<AppUser> _collectionUsers;
    private readonly ILogger<UserRepository> _logger; // TODO fix type

    // constructor - dependency injections
    public LikeRepository(
        IMongoClient client, IMongoDbSettings dbSettings,
        IUserRepository userRepository, ILogger<UserRepository> logger
        )
    {
        _client = client; // used for Session
        var dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<Like>("likes");
        _collectionUsers = dbName.GetCollection<AppUser>("users");

        _logger = logger;
    }
    #endregion

    async Task<LikeStatus> ILikeRepository.AddLikeAsync(string? loggedInUserEmail, string targetMemberEmail, CancellationToken cancellationToken)
    {
        // TODO break down to functions
        LikeStatus likeStatus = new();

        AppUser? loggedInAppUser = await _collectionUsers.Find<AppUser>(appUser => appUser.Email == loggedInUserEmail).FirstOrDefaultAsync(cancellationToken);
        AppUser? targetMemberAppUser = await _collectionUsers.Find<AppUser>(appUser => appUser.Email == targetMemberEmail).FirstOrDefaultAsync(cancellationToken);

        // TODO implement already likes
        bool doesExist = await _collection.Find<Like>(like =>
            like.LoggedInUserId == loggedInAppUser.Id
            && like.TargetMemberId == targetMemberAppUser.Id)
            .AnyAsync(cancellationToken: cancellationToken);

        if (doesExist)
        {
            likeStatus.IsAlreadyLiked = true;
            return likeStatus;
        }

        if (!(loggedInAppUser.Id is null || targetMemberAppUser.Id is null))
        {
            // likes.Add()
            Like? like = Mappers.ConvertAppUsertoLike(loggedInAppUser.Id, targetMemberAppUser.Id);

            //// Session is NOT supported in MongoDb Standalone servers!
            // Create a session object that is used when leveraging transactions
            using var session = await _client.StartSessionAsync(null, cancellationToken);

            // Begin transaction
            session.StartTransaction();

            if (like is not null)
                try
                {
                    // TODO add session to this part so if UpdateLikedByCount failed, undo the InsertOnce.
                    await _collection.InsertOneAsync(session, like, null, cancellationToken);

                    await UpdateLikedByCount(session, targetMemberAppUser.Id, cancellationToken);

                    await session.CommitTransactionAsync(cancellationToken);

                    likeStatus.IsSuccess = true;
                    return likeStatus; // success
                }
                catch (System.Exception ex)
                {
                    await session.AbortTransactionAsync(cancellationToken);

                    _logger.LogError("Like failed. Error writing to MongoDB" + ex.Message);

                    return likeStatus; // all false
                }
        }

        return likeStatus; // Faild
    }

    async Task<List<MemberDto>> ILikeRepository.GetLikedMembersAsync(string? loggedInUserEmail, string predicate, CancellationToken cancellationToken)
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
