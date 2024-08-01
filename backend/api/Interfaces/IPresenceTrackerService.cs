namespace api.Interfaces;

public interface IPresenceTrackerService
{
    public Task CheckUserConnectedAsync(string userName, string connectionId);
    public Task CheckUserDisconnectedAsync(string userName, string connectionId);
    // public Task<IEnumerable<string>> GetOnlineUsersAsync();
}
