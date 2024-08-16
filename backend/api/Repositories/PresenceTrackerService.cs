namespace api.Repositories;

public class PresenceTrackerService : IPresenceTrackerService
{
    private readonly IMongoCollection<PresenceTracker> _collection;
    private readonly ILogger<PresenceTrackerService> _logger;

    public PresenceTrackerService(IMongoClient client, IMyMongoDbSettings dbSettings, ILogger<PresenceTrackerService> logger)
    {
        IMongoDatabase? dbName = client.GetDatabase(dbSettings.DatabaseName) ?? throw new ArgumentNullException(nameof(dbName));
        _collection = dbName.GetCollection<PresenceTracker>(AppVariablesExtensions.collectionOnlineTrackers);
        _logger = logger;
    }

    public async Task SaveConnectedUserAsync(string userName, string connectionId)
    {
        try
        {
            bool doesTrackerExist = await _collection.Find(doc => doc.UserName == userName).AnyAsync();

            if (doesTrackerExist) // User is already connected. Add the newer connection
            {
                UpdateDefinition<PresenceTracker> updateDefinition = Builders<PresenceTracker>.Update
                    .Set(appUser => appUser.Schema, AppVariablesExtensions.AppVersions.Last<string>())
                    .AddToSet(doc => doc.ConnectionIds, connectionId);

                await _collection.UpdateOneAsync(doc => doc.UserName == userName, updateDefinition);
            }
            else  // First connection of the user
            {
                PresenceTracker tracker = new()
                {
                    Schema = AppVariablesExtensions.AppVersions.Last<string>(),
                    UserName = userName
                };
                tracker.ConnectionIds.Add(connectionId);

                await _collection.InsertOneAsync(tracker);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex.StackTrace, ex.Message);
        }
    }

    public async Task<bool> CheckIfUserIsOnlineAsync(string userName) =>
        await _collection.Find<PresenceTracker>(doc => doc.UserName == userName).AnyAsync();

    public async Task<IEnumerable<string>> GetOnlineUserNamesAsync() =>
        await _collection.AsQueryable()
            .Select(doc => doc.UserName)
            .ToListAsync();

    public async Task RemoveDisconnectedUserAsync(string userName, string connectionId)
    {
        try
        {
            PresenceTracker? tracker = await _collection.Find(doc => doc.UserName == userName).FirstOrDefaultAsync();

            if (tracker is not null && tracker.ConnectionIds.Count > 1)
            {
                tracker.ConnectionIds.Remove(connectionId);

                UpdateDefinition<PresenceTracker> updateDefinition = Builders<PresenceTracker>.Update
                    .Set(doc => doc.ConnectionIds, tracker.ConnectionIds);

                await _collection.UpdateOneAsync(doc => doc.UserName == userName, updateDefinition);
            }
            else
            {
                await _collection.DeleteOneAsync(doc => doc.UserName == userName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex.StackTrace, ex.Message);
        }
    }
}