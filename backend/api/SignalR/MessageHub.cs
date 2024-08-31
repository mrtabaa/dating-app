namespace api.SignalR;

[Authorize]
public class MessageHub(
    IMessageRepository _messageRepository, ITokenService _tokenService, ILogger<MessageHub> _logger) : Hub
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
        _logger.LogWarning("CREATE: " + messageInDto);

        const string NewMessageRes = "NewMessageRes";

        HttpContext httpContext = Context.GetHttpContext()
            ?? throw new HubException("httpContext cannot be null!");

        _logger.LogWarning("HTTPCONTEXT");

        CancellationToken cancellationToken = httpContext.RequestAborted;

        ObjectId userId = await _tokenService.GetActualUserIdAsync(Context.User?.GetUserIdHashed(), cancellationToken)
            ?? throw new HubException("User id is invalid. Login again.");

        MessageDto messageDto = await _messageRepository.CreateAsync(userId, messageInDto, cancellationToken)
            ?? throw new HubException("Message creation failed. Try again.");

        _logger.LogWarning("MESSAGEDTO: " + messageDto);

        string groupName = GetGroupName(GetUserName(), messageInDto.ReceiverUserName.ToUpper());

        _logger.LogWarning("GROUPNAME: " + groupName);

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
