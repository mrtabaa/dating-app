
namespace api.Repositories;

public class MessageRepository : IMessageRepository
{
    #region Db and Token Settings
    private readonly IMongoCollection<Message> _collection;
    private readonly IUserRepository _userRepository;
    private readonly IPhotoService _photoService;
    private readonly ILogger<UserRepository> _logger;

    // constructor - dependency injections
    public MessageRepository(
        IMongoClient client, IMyMongoDbSettings dbSettings,
        IUserRepository userRepository, IPhotoService photoService,
        ILogger<UserRepository> logger
        )
    {
        var dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<Message>(AppVariablesExtensions.collectionMessages);
        _userRepository = userRepository;
        _photoService = photoService;
        _logger = logger;
    }
    #endregion Db and Token Settings

    #region CRUD
    public async Task<MessageStatus> CreateAsync(ObjectId userId, MessageInDto messageInDto, CancellationToken cancellationToken)
    {
        ObjectId? targetUserId = await _userRepository.GetIdByUserNameAsync(messageInDto.ReceiverUserName, cancellationToken);

        if (targetUserId is null)
            return new MessageStatus(IsReceiverNotFound: true);

        AppUser? loggedInUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (loggedInUser is null)
            return new MessageStatus(IsLoggedInUserNotFound: true);

        Message message = Mappers.ConvertMessageInDtoToMessage(messageInDto.Content, userId, targetUserId.Value);

        await _collection.InsertOneAsync(message, null, cancellationToken);

        // Convert all targetMember profile photo to blob Sas format
        string? profilePhotoUrl = loggedInUser.Photos.FirstOrDefault(photo => photo.IsMain)?.Url_165;

        string? profilePhotoSasUrl = _photoService.ConvertPhotoUrlToBlobLinkWithSas(profilePhotoUrl);

        MessageDto messageDto = Mappers.ConvertMessageToMessageDto(message, loggedInUser, profilePhotoSasUrl);

        return new MessageStatus(MessageDto: messageDto);
    }

    public async Task<PagedList<Message>> GetAsync(ObjectId userId, MessageParams messageParams, CancellationToken cancellationToken)
    {
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

        return await PagedList<Message>.CreatePagedListDescendingAsync(query, messageParams.PageNumber, messageParams.PageSize, cancellationToken);
    }

    public async Task<PagedList<Message>?> GetThreadAsync(ObjectId userId, MessageParams messageParams, CancellationToken cancellationToken)
    {
        ObjectId? targetUserId = await _userRepository.GetIdByUserNameAsync(messageParams.TargetUserName, cancellationToken);

        if (targetUserId == null)
            return null;

        IMongoQueryable<Message> query = _collection.AsQueryable()
            .Where(doc =>
                (doc.SenderId == userId && doc.RecieverId == targetUserId) ||
                (doc.RecieverId == userId && doc.SenderId == targetUserId)
            )
            .OrderBy(doc => doc.SentOn);

        return await PagedList<Message>.CreatePagedListDescendingAsync(query, messageParams.PageNumber, messageParams.PageSize, cancellationToken);
    }
    #endregion CRUD
}
