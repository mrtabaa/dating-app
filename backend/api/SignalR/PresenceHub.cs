namespace api.SignalR;

public class PresenceHub(IPresenceTrackerService _presenceTrackerService) : Hub
{
    private const string _CheckUserIsOnline = "CheckUserIsOnline";
    private const string _CheckUserIsOffline = "CheckUserIsOffline";
    private const string _GetOnlineUsers = "GetOnlineUsers";

    public override async Task OnConnectedAsync()
    {
        string? userName = Context.User?.GetUserName();
        if (!string.IsNullOrEmpty(userName))
        {
            await _presenceTrackerService.CheckUserConnectedAsync(userName, Context.ConnectionId);

            await Clients.Others.SendAsync(_CheckUserIsOnline, userName);

            // IEnumerable<string> onlineUsers = await _presenceTrackerService.GetOnlineUsersAsync();

            // await Clients.All.SendAsync(_GetOnlineUsers, onlineUsers);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string? userName = Context.User?.GetUserName();
        if (!string.IsNullOrEmpty(userName))
        {
            await _presenceTrackerService.CheckUserDisconnectedAsync(userName, Context.ConnectionId);

            await Clients.Others.SendAsync(_CheckUserIsOffline, userName);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
