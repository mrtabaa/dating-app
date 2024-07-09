
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
        ObjectId? receiverId = await _userRepository.GetIdByUserNameAsync(messageInDto.ReceiverUserName, cancellationToken);

        if (receiverId is null)
        {
            return new MessageStatus(IsReceiverNotFound: true);
        }

        Message message = Mappers.ConvertMessageInDtoToMessage(messageInDto.Content, userId, receiverId.Value);

        await _collection.InsertOneAsync(message, null, cancellationToken);

        return new MessageStatus(IsSuccess: true);
    }

    public async Task<PagedList<Message>> GetInboxMessagesAsync(ObjectId userId, PaginationParams paginationParams, CancellationToken cancellationToken)
    {
        MessageStatus messageStatus = new();

        IMongoQueryable<Message> query = _collection.AsQueryable()
            .Where(doc => doc.SenderId == userId || doc.RecieverId == userId)
            .OrderByDescending(doc => doc.SentOn);

        return await PagedList<Message>.CreatePagedListAsync(query, paginationParams.PageNumber, paginationParams.PageSize, cancellationToken);
    }
    #endregion CRUD
}
