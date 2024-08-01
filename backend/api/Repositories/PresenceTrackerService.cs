namespace api.Repositories;

public class PresenceTrackerService : IPresenceTrackerService
{
    private readonly IMongoCollection<PresenceTracker> _collection;
    private readonly ILogger<PresenceTrackerService> _logger;

    public PresenceTrackerService(IMongoClient client, IMyMongoDbSettings dbSettings, ILogger<PresenceTrackerService> logger)
    {
        var dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<PresenceTracker>(AppVariablesExtensions.collectionOnlineTrackers);
        _logger = logger;
    }

    public async Task CheckUserConnectedAsync(string userName, string connectionId)
    {
        try
        {
            PresenceTracker? tracker = await _collection.Find(doc => doc.UserName == userName).FirstOrDefaultAsync();

            if (tracker is not null) // User is already connected. Add the newer connection
            {
                tracker.ConnectionIds.Add(connectionId);

                UpdateDefinition<PresenceTracker> updateDefinition = Builders<PresenceTracker>.Update
                    .Set(doc => doc.ConnectionIds, tracker.ConnectionIds);

                await _collection.UpdateOneAsync(doc => doc.UserName == userName, updateDefinition);
            }
            else  // First connection of the user
            {
                tracker = new()
                {
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

    public async Task CheckUserDisconnectedAsync(string userName, string connectionId)
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

    // public async Task<IEnumerable<string>> GetOnlineUsersAsync()
    // {
    //     return await _collection.AsQueryable()
    //         .Select(doc => doc.UserName)
    //         .ToListAsync();
    // }
}