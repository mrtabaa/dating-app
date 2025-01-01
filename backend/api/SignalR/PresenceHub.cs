namespace api.SignalR;

[Authorize]
public class PresenceHub(IPresenceTrackerService presenceTrackerService, ITokenService tokenService, IUserRepository userRepository) : Hub
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
            // Do NOT use CancellationToken for this request or the ConnectionId does NOT get removed from DB on browser closed.
            await presenceTrackerService.RemoveDisconnectedUserAsync(await GetUserName(), Context.ConnectionId);

            // await Clients.Others.SendAsync(_CheckUserIsOffline, userName, cancellationToken);

            IEnumerable<OnlineUsersDto> onlineUsersDtos = await presenceTrackerService.GetOnlineUsersDtosAsync();

            await Clients.All.SendAsync(SignalRMessages.GetOnlineUsers, onlineUsersDtos);

        await base.OnDisconnectedAsync(exception);
    }
    
    private async Task<string> GetUserName()
    {
        string userIdHashed = Context.User?.GetUserIdHashed()
                              ?? throw new HubException("The token is invalid/expired. Login again.");

        return await userRepository.GetUserNameByIdentifierHashAsync(userIdHashed, GetCancellationToken())
               ?? throw new HubException("UserName is invalid. Login again.");
    }

    private CancellationToken GetCancellationToken() =>
        Context.GetHttpContext()?.RequestAborted
        ?? throw new HubException("CancellationToken is null which cannot be.");
}