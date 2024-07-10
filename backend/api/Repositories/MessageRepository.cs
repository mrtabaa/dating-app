
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

    public async Task<PagedList<Message>> GetAsync(ObjectId userId, MessageParams messageParams, CancellationToken cancellationToken)
    {
        MessageStatus messageStatus = new();

        IMongoQueryable<Message> query = _collection.AsQueryable()
            .OrderByDescending(doc => doc.SentOn);

        query = messageParams.Predicate switch
        {
            // Group all messages based on SenderId then Select the first one which is the latest message since they're OrderByDescending
            MessagePredicate.Inbox => query
                .Where(doc => doc.RecieverId == userId)
                .GroupBy(doc => doc.SenderId)
                .Select(group => group.First()), // Inbox
            MessagePredicate.Unread => query
                .Where(doc => doc.RecieverId == userId && doc.ReadOn == null)
                .GroupBy(doc => doc.SenderId)
                .Select(group => group.First()), // Unread
            MessagePredicate.Read => query
                .Where(doc => doc.RecieverId == userId && doc.ReadOn != null)
                .GroupBy(doc => doc.SenderId)
                .Select(group => group.First()), // Read
            MessagePredicate.Sent => query
                .Where(doc => doc.SenderId == userId)
                .GroupBy(doc => doc.SenderId)
                .Select(group => group.First()), // Sent
            _ => query
        };

        return await PagedList<Message>.CreatePagedListAsync(query, messageParams.PageNumber, messageParams.PageSize, cancellationToken);
    }
    #endregion CRUD
}
