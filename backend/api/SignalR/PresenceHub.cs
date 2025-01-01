namespace api.SignalR;

[Authorize]
public class PresenceHub(IPresenceTrackerService presenceTrackerService, ITokenService tokenService) : Hub
{
    public override async Task OnConnectedAsync()
    {
        ObjectId? userId = await tokenService.GetActualUserIdAsync(Context.User?.GetUserIdHashed(), GetCancellationToken())
                           ?? throw new ArgumentNullException(nameof(userId), "userId is null");

        await presenceTrackerService.SaveConnectedUserAsync(userId.Value, Context.ConnectionId, GetCancellationToken());

        // await Clients.Others.SendAsync(_CheckUserIsOnline, userName, cancellationToken);

        IEnumerable<OnlineUsersDto> onlineUsersDtos = await presenceTrackerService.GetOnlineUsersDtosAsync(GetCancellationToken());

        await Clients.All.SendAsync(SignalRMessages.GetOnlineUsers, onlineUsersDtos, GetCancellationToken());
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string? userName = Context.User?.GetUserName();
        if (!string.IsNullOrEmpty(userName))
        {
            // Do NOT use CancellationToken for this request or the ConnectionId does NOT get removed from DB on browser closed.
            await presenceTrackerService.RemoveDisconnectedUserAsync(userName, Context.ConnectionId);

            // await Clients.Others.SendAsync(_CheckUserIsOffline, userName, cancellationToken);

            IEnumerable<OnlineUsersDto> onlineUsersDtos = await presenceTrackerService.GetOnlineUsersDtosAsync();

            await Clients.All.SendAsync(SignalRMessages.GetOnlineUsers, onlineUsersDtos);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private CancellationToken GetCancellationToken() =>
        Context.GetHttpContext()?.RequestAborted
        ?? throw new HubException("CancellationToken is null which cannot be.");
}