namespace api.SignalR;

[Authorize]
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
            await _presenceTrackerService.SaveConnectedUserAsync(userName, Context.ConnectionId);

            await Clients.Others.SendAsync(_CheckUserIsOnline, userName);

            IEnumerable<string> onlineUserNames = await _presenceTrackerService.GetOnlineUserNamesAsync();
            await Clients.All.SendAsync(_GetOnlineUsers, onlineUserNames);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string? userName = Context.User?.GetUserName();
        if (!string.IsNullOrEmpty(userName))
        {
            await _presenceTrackerService.RemoveDisconnectedUserAsync(userName, Context.ConnectionId);

            await Clients.Others.SendAsync(_CheckUserIsOffline, userName);

            IEnumerable<string> onlineUserNames = await _presenceTrackerService.GetOnlineUserNamesAsync();
            await Clients.All.SendAsync(_GetOnlineUsers, onlineUserNames);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
