namespace api.SignalR.Interfaces;

public interface IPresenceTrackerService
{
    public Task SaveConnectedUserAsync(ObjectId userId, string connectionId, CancellationToken cancellationToken);

    // Make CancellationToken optional so it's not used on DB ConnectionId removal 
    public Task<IEnumerable<OnlineUsersDto>> GetOnlineUsersDtosAsync(CancellationToken cancellationToken = default);

    // No CancellationToken for successful DB ConnectionId removal
    public Task RemoveDisconnectedUserAsync(string userName, string connectionId);
}