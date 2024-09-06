namespace api.SignalR.Services;

public class MessageService : IMessageService
{
    #region Fields and constructors
    private readonly IMongoCollection<AppUser> _collection;

    // constructor - dependency injections
    public MessageService(
        IMongoClient client, IMyMongoDbSettings dbSettings
        )
    {
        IMongoDatabase? dbName = client.GetDatabase(dbSettings.DatabaseName) ?? throw new ArgumentNullException(nameof(dbName));
        _collection = dbName.GetCollection<AppUser>(AppVariablesExtensions.collectionUsers);
    }
    #endregion Fields and constructors

    public async Task<bool> AddGroupNameAsync(ObjectId userId, string groupName, CancellationToken cancellationToken)
    {
        // UpdateDefinition<AppUser> updateDefinition = Builders<AppUser>.Update
        //     .AddToSet(appUser => appUser.GroupNames, groupName);

        // UpdateResult updateResult = await _collection.UpdateOneAsync(appUser => appUser.Id == userId, updateDefinition, null, cancellationToken);

        // return updateResult.ModifiedCount > 0;
        return false;
    }

    public async Task<HashSet<string>?> GetGroupNamesAsync(ObjectId userId, CancellationToken cancellationToken) =>
    null;
    // await _collection.AsQueryable()
    //     .Where<AppUser>(appUser => appUser.Id == userId)
    //     .Select(appUser => appUser.GroupNames)
    //     .FirstOrDefaultAsync(cancellationToken);


    public async Task<bool> RemoveGroupNameAsync(ObjectId userId, string groupName, CancellationToken cancellationToken)
    {
        // UpdateDefinition<AppUser> updateDefinition = Builders<AppUser>.Update
        //     .Pull(appUser => appUser.GroupNames, groupName);

        // UpdateResult updateResult = await _collection.UpdateOneAsync(appUser => appUser.Id == userId, updateDefinition, null, cancellationToken);

        // return updateResult.ModifiedCount > 0;
        return false;
    }
}
