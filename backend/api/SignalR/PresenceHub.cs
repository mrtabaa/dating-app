namespace api.SignalR;

[Authorize]
public class PresenceHub(IPresenceTrackerService _presenceTrackerService, ITokenService _tokenService) : Hub
{
    // private const string _CheckUserIsOnline = "CheckUserIsOnline";
    // private const string _CheckUserIsOffline = "CheckUserIsOffline";
    private const string _GetOnlineUsers = "GetOnlineUsers";

    public override async Task OnConnectedAsync()
    {
        HttpContext? httpContext = Context.GetHttpContext();
        if (httpContext == null) return;

        CancellationToken cancellationToken = httpContext.RequestAborted;

        ObjectId? userId = await _tokenService.GetActualUserIdAsync(httpContext.User.GetUserIdHashed(), cancellationToken)
            ?? throw new ArgumentNullException("userId is null", nameof(userId));

        await _presenceTrackerService.SaveConnectedUserAsync(userId.Value, Context.ConnectionId, cancellationToken);

        // await Clients.Others.SendAsync(_CheckUserIsOnline, userName, cancellationToken);

        IEnumerable<OnlineUsersDto> onlineUsersDtos = await _presenceTrackerService.GetOnlineUsersDtosAsync(cancellationToken);

        await Clients.All.SendAsync(_GetOnlineUsers, onlineUsersDtos, cancellationToken);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        HttpContext? httpContext = Context.GetHttpContext();
        if (httpContext == null) return;

        CancellationToken cancellationToken = httpContext.RequestAborted;
        string? userName = Context.User?.GetUserName();
        if (!string.IsNullOrEmpty(userName))
        {
            await _presenceTrackerService.RemoveDisconnectedUserAsync(userName, Context.ConnectionId, cancellationToken);

            // await Clients.Others.SendAsync(_CheckUserIsOffline, userName, cancellationToken);

            IEnumerable<OnlineUsersDto> onlineUsersDtos = await _presenceTrackerService.GetOnlineUsersDtosAsync(cancellationToken);

            await Clients.All.SendAsync(_GetOnlineUsers, onlineUsersDtos, cancellationToken);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
