namespace api.SignalR.Services;

public class MessageService : IMessageService
{
    public async Task<bool> AddMessageGroupAsync(ObjectId userId, MessageGroup messageGroup, CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser>? updateDefinition = Builders<AppUser>.Update
            .AddToSet(appUser => appUser.MessageGroups, messageGroup);

        UpdateResult? updateResult = await _collection.UpdateOneAsync(appUser => appUser.Id == userId, updateDefinition,
            null,
            cancellationToken);

        return updateResult.MatchedCount > 0;
    }

    public async Task<MessageGroup?> GetMessageGroupAsync(ObjectId userId, string connectionId, CancellationToken cancellationToken) =>
        await _collection.AsQueryable()
            .Where(appUser => appUser.Id == userId)
            .SelectMany(appUser => appUser.MessageGroups)
            .FirstOrDefaultAsync(mG => mG.ConnectionId == connectionId, cancellationToken);

    public async Task<bool> RemoveMessageGroupAsync(ObjectId userId, MessageGroup messageGroup)
    {
        UpdateDefinition<AppUser> updateDefinition = Builders<AppUser>.Update
            .Pull(appUser => appUser.MessageGroups, messageGroup);
        UpdateResult updateResult = await _collection.UpdateOneAsync(appUser => appUser.Id == userId, updateDefinition);
        return updateResult.ModifiedCount > 0;
    }

    public async Task<bool> CheckIsMemberIsInGroupAsync(ObjectId userId, string groupNameIn, CancellationToken cancellationToken) =>
        await _collection.AsQueryable()
            .Where(appUser => appUser.Id == userId)
            .SelectMany(appUser => appUser.MessageGroups)
            .AnyAsync(mG => mG.Name == groupNameIn, cancellationToken);

    #region Fields and constructors

    private readonly IMongoCollection<AppUser> _collection;

    // constructor - dependency injections
    public MessageService(
        IMongoClient client, IMyMongoDbSettings dbSettings
    )
    {
        IMongoDatabase? dbName = client.GetDatabase(dbSettings.DatabaseName) ??
                                 throw new ArgumentNullException(nameof(dbName));
        _collection = dbName.GetCollection<AppUser>(AppVariablesExtensions.collectionUsers);
    }

    #endregion Fields and constructors
}