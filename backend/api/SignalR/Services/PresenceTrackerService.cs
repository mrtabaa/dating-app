namespace api.SignalR.Services;

public class PresenceTrackerService : IPresenceTrackerService
{
    #region Fields and constructors
    private readonly IMongoCollection<AppUser> _collection;

    public PresenceTrackerService(IMongoClient client, IMyMongoDbSettings dbSettings)
    {
        IMongoDatabase? dbName = client.GetDatabase(dbSettings.DatabaseName) ?? throw new ArgumentNullException(nameof(dbName));
        _collection = dbName.GetCollection<AppUser>(AppVariablesExtensions.collectionUsers);
    }
    #endregion Fields and constructors

    public async Task SaveConnectedUserAsync(ObjectId userId, string connectionId, CancellationToken cancellationToken)
    {
        bool isProfileCompleted = await _collection.AsQueryable<AppUser>()
            .Where(appUser => appUser.Id == userId)
            .Select(appUser => appUser.IsProfileCompleted)
            .FirstAsync(cancellationToken);

        if (!isProfileCompleted) return;

        UpdateDefinition<AppUser> updateDefinition = Builders<AppUser>.Update
            .AddToSet(appUser => appUser.ConnectionsPresence, connectionId);

        await _collection.UpdateOneAsync(appUser => appUser.Id == userId, updateDefinition, null, cancellationToken);
    }

    public async Task<IEnumerable<OnlineUsersDto>> GetOnlineUsersDtosAsync(CancellationToken cancellationToken)
    {
        IEnumerable<AppUser> appUsers = await _collection.Find(appUser => appUser.ConnectionsPresence.Any()).ToListAsync(cancellationToken);

        List<OnlineUsersDto> onlineUsersDtos = [];

        foreach (AppUser appUser in appUsers)
        {
            onlineUsersDtos.Add(Mappers.ConvertAppUserToOnlineStatusDto(appUser));
        }

        return onlineUsersDtos;
    }

    public async Task RemoveDisconnectedUserAsync(string userName, string connectionId)
    {
        UpdateDefinition<AppUser> updateDefinition = Builders<AppUser>.Update
            .Pull(appUser => appUser.ConnectionsPresence, connectionId);

        await _collection.UpdateOneAsync(appUser => appUser.NormalizedUserName == userName, updateDefinition);
    }
}