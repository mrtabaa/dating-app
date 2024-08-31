namespace api.SignalR;

[Authorize]
public class MessageHub(IMessageRepository _messageRepository, ITokenService _tokenService) : Hub
{
    public async Task JoinGroup(string targetUserName)
    {
        string groupName = GetGroupName(GetUserName(), targetUserName.ToUpper());

        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it appropriately
            throw new HubException("An error occurred while joining the group.", ex);
        }
    }

    public async Task Create(MessageInDto messageInDto)
    {
        const string NewMessageRes = "NewMessageRes";

        HttpContext httpContext = Context.GetHttpContext()
            ?? throw new HubException("httpContext cannot be null!");

        CancellationToken cancellationToken = httpContext.RequestAborted;

        ObjectId userId = await _tokenService.GetActualUserIdAsync(Context.User?.GetUserIdHashed(), cancellationToken)
            ?? throw new HubException("UserId is invalid. Login again.");

        MessageDto? messageDto = await _messageRepository.CreateAsync(userId, messageInDto, cancellationToken)
            ?? throw new HubException("MessageDto is null. Message creation failed. Try again.");

        string groupName = GetGroupName(GetUserName(), messageInDto.ReceiverUserName.ToUpper());

        await Clients.Group(groupName).SendAsync(NewMessageRes, messageDto, cancellationToken);
    }

    private static string GetGroupName(string caller, string other)
    {
        bool stringCompare = string.CompareOrdinal(caller, other) < 0;

        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    private string GetUserName() =>
        Context.User?.GetUserName() ?? throw new HubException("UserName is invalid. Login again.");
}
