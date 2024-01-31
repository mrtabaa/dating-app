
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

    async Task<bool> ILikesRepository.AddLikeAsync(string? loggedInUserId, string targetMemberId, CancellationToken cancellationToken)
    {
        AppUser? targetAppUser = await _userRepository.GetByIdAsync(targetMemberId, cancellationToken);

        if (targetAppUser is not null)
        {
            Like? like = Mappers.ConvertAppUsertoLike(targetAppUser, loggedInUserId);

            if (like is not null)
            {
                await _collection.InsertOneAsync(like);

                return true; // success
            }
        }

        return false; // appUser not found
    }

    // async Task<IEnumerable<Like>> ILikesRepository.GetLikedMembersAsync(string? loggedInUserId, string targetMemberId, string predicate, CancellationToken cancellationToken)
    // {
    //     if (predicate.Equals("liked"))
    //         return await _collection.Find<Like>(like => like.LoggedInUserId == loggedInUserId && like.TargetMemberId == targetMemberId).ToListAsync();

    //     if (predicate.Equals("likedBy"))
    //         return await _collection.Find<Like>(like => like.LoggedInUserId == loggedInUserId && like.TargetMemberId == targetMemberId).ToListAsync();
    // }
}
