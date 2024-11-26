namespace api.SignalR;

[Authorize]
public class PresenceHub(IPresenceTrackerService _presenceTrackerService, ITokenService _tokenService) : Hub
{
    public override async Task OnConnectedAsync()
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(Context.User?.GetUserIdHashed(), GetCancellationToken())
                           ?? throw new ArgumentNullException("userId is null", nameof(userId));

        await _presenceTrackerService.SaveConnectedUserAsync(userId.Value, Context.ConnectionId, GetCancellationToken());

        // await Clients.Others.SendAsync(_CheckUserIsOnline, userName, cancellationToken);

        IEnumerable<OnlineUsersDto> onlineUsersDtos = await _presenceTrackerService.GetOnlineUsersDtosAsync(GetCancellationToken());

        await Clients.All.SendAsync(SignalRMessages.GetOnlineUsers, onlineUsersDtos, GetCancellationToken());
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string? userName = Context.User?.GetUserName();
        if (!string.IsNullOrEmpty(userName))
        {
            await _presenceTrackerService.RemoveDisconnectedUserAsync(userName, Context.ConnectionId, GetCancellationToken());

            // await Clients.Others.SendAsync(_CheckUserIsOffline, userName, cancellationToken);

            IEnumerable<OnlineUsersDto> onlineUsersDtos = await _presenceTrackerService.GetOnlineUsersDtosAsync(GetCancellationToken());

            await Clients.All.SendAsync(SignalRMessages.GetOnlineUsers, onlineUsersDtos, GetCancellationToken());

            await base.OnDisconnectedAsync(exception);
        }
    }

    private CancellationToken GetCancellationToken() =>
        Context.GetHttpContext()?.RequestAborted
        ?? throw new HubException("CancellationToken is null which cannot be.");
}