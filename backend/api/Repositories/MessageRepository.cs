namespace api.Repositories;

public class MessageRepository : IMessageRepository
{
    #region Db and Token Settings

    private readonly IMongoCollection<Message> _collection;
    private readonly IUserRepository _userRepository;

    // constructor - dependency injections
    public MessageRepository(
        IMongoClient client, IMyMongoDbSettings dbSettings,
        IUserRepository userRepository
    )
    {
        IMongoDatabase dbName = client.GetDatabase(dbSettings.DatabaseName) ?? throw new ArgumentNullException(nameof(dbName));
        _collection = dbName.GetCollection<Message>(AppVariablesExtensions.collectionMessages);
        _userRepository = userRepository;
    }

    #endregion Db and Token Settings

    #region CRUD

    public async Task<MessageDto?> CreateAsync(ObjectId userId, MessageInDto messageInDto, CancellationToken cancellationToken)
    {
        AppUser? loggedInUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (loggedInUser is null) return null;

        ObjectId? receiverId = await _userRepository.GetIdByUserNameAsync(messageInDto.ReceiverUserName, cancellationToken);
        if (receiverId is null) return null;

        Message message = Mappers.ConvertMessageInDtoToMessage(messageInDto.Content, userId, receiverId.Value);

        string? profilePhotoUrl = await _userRepository.GetProfilePhotoUrlBlobAsync(userId, cancellationToken);

        await _collection.InsertOneAsync(message, null, cancellationToken);

        return Mappers.ConvertMessageToMessageDto(message, loggedInUser, profilePhotoUrl);
    }

    public async Task<PagedList<Message>> GetAsync(ObjectId userId, MessageParams messageParams, CancellationToken cancellationToken)
    {
        IMongoQueryable<Message> query = _collection.AsQueryable();

        query = messageParams.Predicate switch
        {
            // Group all messages based on SenderId then Select the first one which is the latest message since they're OrderByDescending
            MessagePredicate.Inbox => query
                .Where(doc => doc.ReceiverId == userId)
                .GroupBy(doc => doc.SenderId)
                .Select(group => group.First())
                .OrderByDescending(doc => doc.SentOn),
            MessagePredicate.Unread => query
                .Where(doc => doc.ReceiverId == userId && doc.ReadOn == null)
                .GroupBy(doc => doc.SenderId)
                .Select(group => group.First())
                .OrderByDescending(doc => doc.SentOn),
            MessagePredicate.Read => query
                .Where(doc => doc.ReceiverId == userId && doc.ReadOn != null)
                .GroupBy(doc => doc.SenderId)
                .Select(group => group.First())
                .OrderByDescending(doc => doc.SentOn),
            MessagePredicate.Sent => query
                .Where(doc => doc.SenderId == userId)
                .GroupBy(doc => doc.ReceiverId)
                .Select(group => group.First())
                .OrderByDescending(doc => doc.SentOn),
            _ => query
        };

        return await PagedList<Message>.CreatePagedListAsync(query, messageParams.PageNumber, messageParams.PageSize, cancellationToken);
    }

    public async Task<PagedList<Message>?> GetThreadAsync(ObjectId userId, MessageParams messageParams, CancellationToken cancellationToken)
    {
        ObjectId? targetUserId = await _userRepository.GetIdByUserNameAsync(messageParams.TargetUserName, cancellationToken);

        if (targetUserId == null)
            return null;

        IMongoQueryable<Message> query = _collection.AsQueryable()
            .Where(doc =>
                (doc.SenderId == userId && doc.ReceiverId == targetUserId) ||
                (doc.ReceiverId == userId && doc.SenderId == targetUserId)
            ).OrderByDescending(doc => doc.SentOn);

        PagedList<Message> pagedMessages = await PagedList<Message>.CreatePagedListAsync(query, messageParams.PageNumber, messageParams.PageSize, cancellationToken);

        // update ReadOn
        if (pagedMessages.Count > 0)
            await UpdateReadOnAsync(userId, targetUserId.Value, cancellationToken);

        return pagedMessages;
    }

    public async Task<DateTime> UpdateReadOnAsync(ObjectId userId, ObjectId targetUserId, CancellationToken cancellationToken)
    {
        DateTime readOnTimestamp = DateTime.UtcNow;
        
        FilterDefinition<Message>? filter = Builders<Message>.Filter
            .Where(doc =>
                doc.ReceiverId == userId && doc.SenderId == targetUserId
                                         && doc.ReadOn == null
            );

        UpdateDefinition<Message> updateDefReadOn = Builders<Message>.Update
            .Set(message => message.ReadOn, readOnTimestamp);

        await _collection.UpdateManyAsync(filter, updateDefReadOn, null, cancellationToken);

        return readOnTimestamp;
    }

    #endregion CRUD
}