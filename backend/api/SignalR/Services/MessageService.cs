namespace api.SignalR.Services;

public class MessageService : IMessageService
{
    public async Task<bool> AddGroupNameAsync(ObjectId userId, string groupName, CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser>? updateDefinition = Builders<AppUser>.Update
            .AddToSet(appUser => appUser.MessageGroups, groupName);

        UpdateResult? updateResult = await _collection.UpdateOneAsync(appUser => appUser.Id == userId, updateDefinition,
            null,
            cancellationToken);

        return updateResult.MatchedCount == 1;
    }

    public async Task<bool> RemoveGroupNameFromDbAsync(ObjectId userId, string groupName, CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser> updateDefinition = Builders<AppUser>.Update
            .Pull(appUser => appUser.MessageGroups, groupName);
        UpdateResult updateResult = await _collection.UpdateOneAsync(appUser => appUser.Id == userId, updateDefinition, null, cancellationToken);
        return updateResult.ModifiedCount > 0;
    }

    public async Task<bool> CheckIsMemberIsInGroupAsync(ObjectId userId, string groupNameIn, CancellationToken cancellationToken) =>
        await _collection.AsQueryable()
            .Where(appUser => appUser.Id == userId)
            .SelectMany(appUser => appUser.MessageGroups)
            .AnyAsync(groupName => groupName == groupNameIn, cancellationToken);

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