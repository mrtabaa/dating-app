
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

    async Task<LikeStatus> ILikesRepository.AddLikeAsync(string? loggedInUserId, string targetMemberId, CancellationToken cancellationToken)
    {
        LikeStatus likeStatus = new();

        bool doesExist = await _collection.Find<Like>(like =>
            like.LoggedInUser.LoggedInUserId == loggedInUserId
            && like.TargetMember.TargetMemberId == targetMemberId)
            .AnyAsync(cancellationToken: cancellationToken);

        if (doesExist)
        {
            likeStatus.IsAlreadyLiked = true;
            return likeStatus;
        }

        AppUser? loggedInUserAppUser = await _userRepository.GetByIdAsync(loggedInUserId, cancellationToken);
        AppUser? targetMemberAppUser = await _userRepository.GetByIdAsync(targetMemberId, cancellationToken);

        if (!(loggedInUserAppUser is null || targetMemberAppUser is null || loggedInUserId is null))
        {
            Like? like = Mappers.ConvertAppUsertoLike(loggedInUserAppUser, targetMemberAppUser, loggedInUserId);

            if (like is not null)
            {
                try
                {
                    // TODO add session to this part so if UpdateLikedByCount failed, undo the InsertOnce.
                    await _collection.InsertOneAsync(like, null, cancellationToken);

                    UpdateResult? updateResult = await UpdateLikedByCount(targetMemberId, cancellationToken);

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

    // TODO Add DTOs to not return extra items like other side of the like
    async Task<List<Like>?> ILikesRepository.GetLikedMembersAsync(string? loggedInUserId, string predicate, CancellationToken cancellationToken)
    {
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
    private async Task<UpdateResult?> UpdateLikedByCount(string targetMemberId, CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser> updateLikedByCount = Builders<AppUser>.Update
        .Inc(appUser => appUser.Liked_byCount, 1); // Increament by 1 for each like

        return await _collectionUsers.UpdateOneAsync<AppUser>(appUser =>
            appUser.Id == targetMemberId, updateLikedByCount, null, cancellationToken);
    }
}
