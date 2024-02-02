
namespace api.Repositories;

public class LikesRepository : ILikesRepository
{
    #region Db and Token Settings
    private readonly IMongoClient _client; // used for Session
    private readonly IMongoCollection<Like> _collection;
    private readonly IMongoCollection<AppUser> _collectionUsers;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserRepository> _logger;

    // constructor - dependency injections
    public LikesRepository(
        IMongoClient client, IMongoDbSettings dbSettings,
        IUserRepository userRepository, ILogger<UserRepository> logger
        )
    {
        _client = client; // used for Session
        var dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<Like>("likes");
        _collectionUsers = dbName.GetCollection<AppUser>("users");

        _userRepository = userRepository;

        _logger = logger;
    }
    #endregion

    async Task<LikeStatus> ILikesRepository.AddLikeAsync(string? loggedInUserEmail, string targetMemberEmail, CancellationToken cancellationToken)
    {
        LikeStatus likeStatus = new();

        bool doesExist = await _collection.Find<Like>(like =>
            like.LoggedInUser.Email == loggedInUserEmail
            && like.TargetMember.Email == targetMemberEmail)
            .AnyAsync(cancellationToken: cancellationToken);

        if (doesExist)
        {
            likeStatus.IsAlreadyLiked = true;
            return likeStatus;
        }

        AppUser? loggedInUserAppUser = await _userRepository.GetByEmailAsync(loggedInUserEmail, cancellationToken);
        AppUser? targetMemberAppUser = await _userRepository.GetByEmailAsync(targetMemberEmail, cancellationToken);

        if (!(loggedInUserAppUser is null || targetMemberAppUser is null))
        {
            Like? like = Mappers.ConvertAppUsertoLike(loggedInUserAppUser, targetMemberAppUser);

            if (like is not null)
            {
                //// Session is NOT supported in MongoDb Standalone servers!
                // Create a session object that is used when leveraging transactions
                // using var session = await _client.StartSessionAsync(null, cancellationToken);

                // Begin transaction
                // session.StartTransaction();

                try
                {
                    // TODO add session to this part so if UpdateLikedByCount failed, undo the InsertOnce.
                    await _collection.InsertOneAsync(like, null, cancellationToken);

                    await UpdateLikedByCount(targetMemberAppUser.Id, cancellationToken);

                    likeStatus.IsSuccess = true;
                    return likeStatus; // success
                }
                catch (System.Exception ex)
                {
                    // await session.AbortTransactionAsync(cancellationToken);

                    _logger.LogError("Like failed. Error writing to MongoDB" + ex.Message);

                    return likeStatus; // all false
                }
            }
        }

        return likeStatus; // Faild
    }

    async Task<List<LikeDto>> ILikesRepository.GetLikedMembersAsync(string? loggedInUserEmail, string predicate, CancellationToken cancellationToken)
    {
        // First get appUser Id then look for likes by Id instead of Email to improve performance. Searching by ObjectId is more secure and performant than string.
        // TODO replace with ObjectId
        string? loggedInUserId = await _collectionUsers.AsQueryable<AppUser>()
            .Where(appUser => appUser.Email == loggedInUserEmail)
            .Select(appUser => appUser.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (predicate.Equals("liked"))
        {
            IEnumerable<Like> likes = await _collection.Find<Like>(like => like.LoggedInUser.Id == loggedInUserId)
                .ToListAsync(cancellationToken);

            return ConvertLikesToLikeDtos(likes);
        }

        if (predicate.Equals("liked-by"))
        {
            IEnumerable<Like> likes = await _collection.Find<Like>(like => like.TargetMember.Id == loggedInUserId)
                .ToListAsync(cancellationToken);

            return ConvertLikesToLikeDtos(likes);
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
    private async Task UpdateLikedByCount( string? targetMemberId, CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser> updateLikedByCount = Builders<AppUser>.Update
        .Inc(appUser => appUser.Liked_byCount, 1); // Increament by 1 for each like

        await _collectionUsers.UpdateOneAsync<AppUser>(appUser =>
                appUser.Id == targetMemberId, updateLikedByCount, null, cancellationToken);
    }

    /// <summary>
    /// Get the list of Likes and convert them to a list of LikeDto
    /// </summary>
    /// <param name="likes"></param>
    /// <returns>LikeDtos</returns>
    private static List<LikeDto> ConvertLikesToLikeDtos(IEnumerable<Like> likes)
    {
        List<LikeDto> likeDtos = [];

        if (!likes.Any())
            return [];

        foreach (Like like in likes)
        {
            likeDtos.Add(Mappers.ConvertLikeToLikeDto(like));
        }

        return likeDtos;
    }
}
