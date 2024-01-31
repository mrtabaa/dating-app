
namespace api.Repositories;

public class LikesRepository : ILikesRepository
{
    #region Db and Token Settings
    const string _collectionName = "likes";
    private readonly IMongoCollection<Like> _collection;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserRepository> _logger;

    // constructor - dependency injections
    public LikesRepository(
        IMongoClient client, IMongoDbSettings dbSettings,
        IUserRepository userRepository, ILogger<UserRepository> logger
        )
    {
        var dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<Like>(_collectionName);

        _userRepository = userRepository;

        _logger = logger;
    }
    #endregion

    async Task<LikeStatus> ILikesRepository.AddLikeAsync(string? loggedInUserId, string targetMemberId, CancellationToken cancellationToken)
    {
        LikeStatus likeStatus = new();

        bool doesExist = await _collection.Find<Like>(like =>
            like.LoggedInUserId == loggedInUserId
            && like.TargetMemberId == targetMemberId)
            .AnyAsync(cancellationToken: cancellationToken);

        if (doesExist)
        {
            likeStatus.IsAlreadyLiked = true;
            return likeStatus;
        }

        AppUser? targetAppUser = await _userRepository.GetByIdAsync(targetMemberId, cancellationToken);
        if (targetAppUser is not null)
        {
            Like? like = Mappers.ConvertAppUsertoLike(targetAppUser, loggedInUserId);

            if (like is not null)
            {
                try
                {
                    await _collection.InsertOneAsync(like);

                    likeStatus.IsSuccess = true;
                    return likeStatus; // success
                }
                catch (System.Exception)
                {

                    return likeStatus;
                }
            }
        }

        return likeStatus; // Faild
    }

    async Task<List<Like>?> ILikesRepository.GetLikedMembersAsync(string? loggedInUserId, string predicate, CancellationToken cancellationToken)
    {
        if (predicate.Equals("liked"))
            return await _collection.Find<Like>(like =>
                like.LoggedInUserId == loggedInUserId)
                .ToListAsync(cancellationToken);

        if (predicate.Equals("liked-by"))
            return await _collection.Find<Like>(like =>
                like.TargetMemberId == loggedInUserId)
                .ToListAsync(cancellationToken);

        return null;
    }
}
