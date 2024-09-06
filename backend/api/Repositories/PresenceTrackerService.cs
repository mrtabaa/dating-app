namespace api.Repositories;

public class PresenceTrackerService : IPresenceTrackerService
{
    private readonly IMongoCollection<AppUser> _collection;
    private readonly ILogger<PresenceTrackerService> _logger;

    public PresenceTrackerService(IMongoClient client, IMyMongoDbSettings dbSettings, ILogger<PresenceTrackerService> logger)
    {
        IMongoDatabase? dbName = client.GetDatabase(dbSettings.DatabaseName) ?? throw new ArgumentNullException(nameof(dbName));
        _collection = dbName.GetCollection<AppUser>(AppVariablesExtensions.collectionUsers);
        _logger = logger;
    }

    public async Task SaveConnectedUserAsync(ObjectId userId, string connectionId, CancellationToken cancellationToken)
    {
        Connection connection = new(
            ConnectionId: connectionId,
            GroupNames: []
        );

        UpdateDefinition<AppUser> updateDefinition = Builders<AppUser>.Update
            .AddToSet(appUser => appUser.Connections, connection);

        await _collection.UpdateOneAsync(appUser => appUser.Id == userId, updateDefinition, null, cancellationToken);
    }

    public async Task<IEnumerable<OnlineUsersDto>> GetOnlineUsersDtosAsync(CancellationToken cancellationToken)
    {
        IEnumerable<AppUser> appUsers = await _collection.Find(appUser => appUser.Connections.Any()).ToListAsync(cancellationToken);

        List<OnlineUsersDto> onlineUsersDtos = [];

        foreach (AppUser appUser in appUsers)
        {
            onlineUsersDtos.Add(Mappers.ConvertAppUserToOnlineStatusDto(appUser));
        }

        return onlineUsersDtos;
    }

    public async Task RemoveDisconnectedUserAsync(string userName, string connectionId, CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser> updateDefinition = Builders<AppUser>.Update
            .PullFilter(appUser => appUser.Connections, c => c.ConnectionId == connectionId);

        await _collection.UpdateOneAsync(appUser => appUser.NormalizedUserName == userName, updateDefinition, null, cancellationToken);
    }
}