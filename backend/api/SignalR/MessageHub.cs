namespace api.SignalR;

[Authorize]
public class MessageHub(IMessageRepository _messageRepository, IUserRepository _userRepository, ITokenService _tokenService) : Hub
{
    public async Task JoinGroup(string targetUserName)
    {
        HttpContext httpContext = Context.GetHttpContext()
            ?? throw new HubException("httpContext cannot be null!");

        CancellationToken cancellationToken = httpContext.RequestAborted;

        ObjectId userId = await _tokenService.GetActualUserIdAsync(Context.User?.GetUserIdHashed(), cancellationToken)
            ?? throw new HubException("UserId is invalid. Login again.");

        string groupName = GetGroupName(GetUserName(), targetUserName);

        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await UpdateReadOn(userId, targetUserName, cancellationToken);
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

        await UpdateReadOn(userId, messageInDto.ReceiverUserName, cancellationToken);

        string groupName = GetGroupName(GetUserName(), messageInDto.ReceiverUserName);

        await Clients.Group(groupName).SendAsync(NewMessageRes, messageDto, cancellationToken);
    }

    private static string GetGroupName(string caller, string other)
    {
        string otherUpper = other.ToUpper();
        bool stringCompare = string.CompareOrdinal(caller, otherUpper) < 0;

        return stringCompare ? $"{caller}-{otherUpper}" : $"{otherUpper}-{caller}";
    }

    private string GetUserName() =>
        Context.User?.GetUserName() ?? throw new HubException("UserName is invalid. Login again.");

    private async Task UpdateReadOn(ObjectId userId, string ReceiverUserName, CancellationToken cancellationToken)
    {
        ObjectId? ReceiverUserId = await _userRepository.GetIdByUserNameAsync(ReceiverUserName.ToUpper(), cancellationToken)
            ?? throw new HubException("OtherUserId is invalid.");

        await _messageRepository.UpdateReadOn(userId, ReceiverUserId.Value, cancellationToken);
    }
}
