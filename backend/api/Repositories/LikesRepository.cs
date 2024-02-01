
namespace api.Repositories;

public class LikesRepository : ILikesRepository
{
    #region Db and Token Settings
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
                try
                {
                    // TODO add session to this part so if UpdateLikedByCount failed, undo the InsertOnce.
                    await _collection.InsertOneAsync(like, null, cancellationToken);

                    UpdateResult? updateResult = await UpdateLikedByCount(targetMemberAppUser.Id, cancellationToken);

                    likeStatus.IsSuccess = true;
                    return likeStatus; // success
                }
                catch (System.Exception ex)
                {
                    _logger.LogError("Like failed:" + ex.Message);
                    return likeStatus;
                }
            }
        }

        return likeStatus; // Faild
    }

    // TODO Add DTO to remove Id before sending to client
    async Task<List<Like>?> ILikesRepository.GetLikedMembersAsync(string? loggedInUserEmail, string predicate, CancellationToken cancellationToken)
    {
        // First get appUser Id then look for likes by Id instead of Email to improve performance. Searching by ObjectId is more secure and performant than string.
        // TODO replace with ObjectId
        string? loggedInUserId = await _collectionUsers.AsQueryable<AppUser>()
            .Where(appUser => appUser.Email == loggedInUserEmail)
            .Select(appUser => appUser.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (predicate.Equals("liked"))
        {
            return await _collection.Find<Like>(like => like.LoggedInUser.LoggedInUserId == loggedInUserId)
                .ToListAsync(cancellationToken);
        }

        if (predicate.Equals("liked-by"))
            return await _collection.Find<Like>(like => like.TargetMember.TargetMemberId == loggedInUserId)
                .ToListAsync(cancellationToken);

        return null;
    }

    /// <summary>
    /// Increament Liked-byCount of the member who was liked. (TargetMember)
    /// </summary>
    /// <param name="targetMemberId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<UpdateResult?> UpdateLikedByCount(string? targetMemberId, CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser> updateLikedByCount = Builders<AppUser>.Update
        .Inc(appUser => appUser.Liked_byCount, 1); // Increament by 1 for each like

        return await _collectionUsers.UpdateOneAsync<AppUser>(appUser =>
            appUser.Id == targetMemberId, updateLikedByCount, null, cancellationToken);
    }
}
