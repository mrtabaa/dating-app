using System.Web;

namespace api.SignalR;

public class MessageHub(
    IMessageRepository _messageRepository, ITokenService _tokenService) : Hub
{
    public async Task Create(MessageInDto messageInDto, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(Context.User?.GetUserIdHashed(), cancellationToken)
            ?? throw new HubException("User id is invalid. Login again.");

        CreatedMessageDto? createdMessageDto = await _messageRepository.CreateAsync(userId.Value, messageInDto, cancellationToken)
            ?? throw new HubException("Message creation failed. Try again.");

        if (string.IsNullOrEmpty(Context.User?.GetUserName()))
            throw new HubException("UserName is invalid. Login again.");

        string group = GetGroupName(Context.User?.GetUserName() ?? string.Empty, messageInDto.ReceiverUserName);

        await Clients.Group(group).SendAsync("SendMessage", createdMessageDto, cancellationToken);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }

    private static string GetGroupName(string caller, string other)
    {
        bool stringCompare = string.CompareOrdinal(caller, other) < 0;

        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
}
