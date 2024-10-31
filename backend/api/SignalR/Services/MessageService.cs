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
        UpdateDefinition<AppUser> updateDefinition = Builders<AppUser>.Update
           .AddToSet(appUser => appUser.MessageGroups, groupName);

        UpdateResult updateResult = await _collection.UpdateOneAsync<AppUser>(appUser => appUser.Id == userId, updateDefinition, null, cancellationToken);


        // Define the filter to find the user and the correct connection
        // FilterDefinition<AppUser> filter = Builders<AppUser>.Filter.And(
        //     Builders<AppUser>.Filter.Eq(appUser => appUser.Id, userId),
        //     Builders<AppUser>.Filter.Eq("Connections.ConnectionId", connectionId)
        // );

        // bool doesConnectionExist = await _collection.Find<AppUser>(filter).AnyAsync(cancellationToken);

        // UpdateResult updateResult;

        // if (doesConnectionExist)
        // {
        //     // Build the update definition using the positional operator '$' to add groupName to the existing connection
        //     UpdateDefinition<AppUser> updateDefinition = Builders<AppUser>.Update
        //        .AddToSet("Connections.$.GroupNames", groupName);

        //     updateResult = await _collection.UpdateOneAsync(filter, updateDefinition, null, cancellationToken);
        // }
        // else // Add the first ConnectionMessage to the AppUser
        // {
        //     ConnectionMessage connectionMessage = new()
        //     {
        //         ConnectionId = connectionId
        //     };
        //     connectionMessage.GroupNames.Add(groupName);

        //     UpdateDefinition<AppUser> updateDefinition = Builders<AppUser>.Update
        //         .AddToSet(appUser => appUser.ConnectionsMessage, connectionMessage);

        //     updateResult = await _collection.UpdateOneAsync(appUser => appUser.Id == userId, updateDefinition, null, cancellationToken);
        // }

        return updateResult.MatchedCount > 0;
        // return updateResult.MatchedCount > 1; // TODO: Use this after RemoveGroupNameAsync is implemented
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
