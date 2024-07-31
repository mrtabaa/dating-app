namespace api.SignalR;

public class PresenceHub : Hub
{
    private const string _CheckUserIsOnline = "CheckUserIsOnline";
    private const string _CheckUserIsOffline = "CheckUserIsOffline";

    public override async Task OnConnectedAsync()
    {
        await Clients.Others.SendAsync(_CheckUserIsOnline, Context.User?.GetUserName());
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.Others.SendAsync(_CheckUserIsOffline, Context.User?.GetUserName());

        await base.OnDisconnectedAsync(exception);
    }
}
