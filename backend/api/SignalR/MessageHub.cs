namespace api.SignalR;

[Authorize]
public class MessageHub(
    IMessageRepository _messageRepository,
    IMessageService _messageService,
    IUserRepository _userRepository,
    ITokenService _tokenService) : Hub
{
    public async Task JoinGroup(string targetUserName)
    {
        ObjectId userId = await GetUserId();

        string groupName = GetGroupName(GetUserName(), targetUserName);

        await AddGroupNameToDb(userId, groupName);

        // TODO Handle exceptions with middleware
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await GetUpdatedReadOn(userId, targetUserName, groupName);

            await NotifyMembersWhenJoined(groupName);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it appropriately
            throw new HubException("An error occurred while joining the group.", ex);
        }
    }

    // Notify other member when joined back to group
    private async Task NotifyMembersWhenJoined(string groupName)
    {
        await Clients.Group(groupName).SendAsync(SignalRMessages.NotifyMembersOnJoined, GetCancellationToken());
    }

    private async Task GetUpdatedReadOn(ObjectId userId, string targetUserName, string groupName)
    {
        DateTime? updatedOn = await UpdateReadOnAsync(userId, targetUserName, GetCancellationToken());

        if (updatedOn is not null)
            await Clients.Group(groupName).SendAsync(SignalRMessages.UpdatedReadOn, updatedOn, GetCancellationToken());
    }

    public async Task Create(MessageInDto messageInDto)
    {
        ObjectId userId = await GetUserId();

        MessageDto messageDto = await _messageRepository.CreateAsync(userId, messageInDto, GetCancellationToken())
                                ?? throw new HubException("MessageDto is null. Message creation failed. Try again.");

        _ = messageDto.UserOrTargetUserName
            ?? throw new HubException("UserOrTargetUserName is null. Message creation failed. Try again.");

        string groupName = GetGroupName(GetUserName(), messageInDto.ReceiverUserName);

        await Clients.Group(groupName).SendAsync(SignalRMessages.NewMessageRes, messageDto, GetCancellationToken());

        ObjectId receiverUserId = await _userRepository.GetIdByUserNameAsync(messageInDto.ReceiverUserName, GetCancellationToken())
                                  ?? throw new HubException("OtherUserId is invalid.");

        if (await _messageService.CheckIsMemberInGroupAsync(receiverUserId, groupName, GetCancellationToken()))
            await GetUpdatedReadOn(userId, messageDto.UserOrTargetUserName, groupName);
    }

    public async Task LeaveGroup(string targetUserName)
    {
        string groupName = GetGroupName(GetUserName(), targetUserName);

        await RemoveGroupNameFromDb(await GetUserId(), groupName);

        // TODO Handle exceptions with middleware
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it appropriately
            throw new HubException("An error occurred while leaving the group.", ex);
        }
    }

    private async Task<ObjectId> GetUserId() =>
        await _tokenService.GetActualUserIdAsync(Context.User?.GetUserIdHashed(), GetCancellationToken())
        ?? throw new HubException("UserId is invalid. Login again.");

    private static string GetGroupName(string caller, string other)
    {
        string otherUpper = other.ToUpper();
        bool stringCompare = string.CompareOrdinal(caller, otherUpper) < 0;

        return stringCompare ? $"{caller}-{otherUpper}" : $"{otherUpper}-{caller}";
    }

    private string GetUserName() =>
        Context.User?.GetUserName() ?? throw new HubException("UserName is invalid. Login again.");

    private async Task<DateTime?> UpdateReadOnAsync(ObjectId userId, string receiverUserName, CancellationToken cancellationToken)
    {
        ObjectId receiverUserId = await _userRepository.GetIdByUserNameAsync(receiverUserName.ToUpper(), cancellationToken)
                                  ?? throw new HubException("OtherUserId is invalid.");

        return await _messageRepository.UpdateReadOnAsync(userId, receiverUserId, cancellationToken);
    }

    private async Task AddGroupNameToDb(ObjectId userId, string groupName)
    {
        if (!await _messageService.AddGroupNameAsync(userId, groupName, GetCancellationToken()))
            throw new HubException("Add groupName to DB failed.");
    }

    private async Task RemoveGroupNameFromDb(ObjectId userId, string groupName)
    {
        if (!await _messageService.RemoveGroupNameFromDbAsync(userId, groupName, GetCancellationToken()))
            throw new HubException("Removing groupName from DB failed.");
    }

    private CancellationToken GetCancellationToken() =>
        Context.GetHttpContext()?.RequestAborted
        ?? throw new HubException("CancellationToken is null but cannot be.");
}