namespace api.Interfaces;

public interface IPresenceTrackerService
{
    public Task SaveConnectedUserAsync(string userName, string connectionId);
    public Task<bool> CheckIfUserIsOnlineAsync(string userName);
    public Task<IEnumerable<string>> GetOnlineUserNamesAsync();
    public Task RemoveDisconnectedUserAsync(string userName, string connectionId);
}
