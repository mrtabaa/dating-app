
namespace api.Repositories;

public class MessageRepository : IMessageRepository
{
    #region Db and Token Settings
    private readonly IMongoCollection<Message> _collection;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserRepository> _logger;

    // constructor - dependency injections
    public MessageRepository(
        IMongoClient client, IMyMongoDbSettings dbSettings,
        IUserRepository userRepository,
        ILogger<UserRepository> logger
        )
    {
        var dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<Message>(AppVariablesExtensions.collectionMessages);
        _userRepository = userRepository;
        _logger = logger;
    }
    #endregion Db and Token Settings

    #region CRUD
    public async Task<MessageStatus> CreateAsync(ObjectId userId, MessageInDto messageInDto, CancellationToken cancellationToken)
    {
        AppUser? loggedInAppUser = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (loggedInAppUser is null)
        {
            return new MessageStatus(IsUnauthorized: true);
        }

        AppUser? targetMemberAppUser = await _userRepository.GetByUserNameAsync(messageInDto.TargetMemberUserName, cancellationToken);

        if (targetMemberAppUser is null)
        {
            return new MessageStatus(IsTargetMemberNotFound: true);
        }

        Message message = Mappers.ConvertMessageInDtoToMessage(messageInDto.Content, loggedInAppUser, targetMemberAppUser);

        await _collection.InsertOneAsync(message, null, cancellationToken);

        return new MessageStatus(IsSuccess: true);
    }
    #endregion CRUD
}
