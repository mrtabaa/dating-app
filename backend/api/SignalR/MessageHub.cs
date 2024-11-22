namespace api.SignalR;

[Authorize]
public class MessageHub(
    IMessageRepository _messageRepository,
    IMessageService _messageService,
    IUserRepository _userRepository,
    ITokenService _tokenService) : Hub
{
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await RemoveGroupNameFromDb();

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGroup(string targetUserName)
    {
        ObjectId userId =
            await _tokenService.GetActualUserIdAsync(Context.User?.GetUserIdHashed(), GetCancellationToken())
            ?? throw new HubException("UserId is invalid. Login again.");

        string groupName = GetGroupName(GetUserName(), targetUserName);

        await AddGroupNameToDb(userId, groupName, GetCancellationToken());

        // TODO Handle exceptions with middleware
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await GetUpdatedReadOn(userId, targetUserName, groupName);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it appropriately
            throw new HubException("An error occurred while joining the group.", ex);
        }
    }

    private async Task GetUpdatedReadOn(ObjectId userId, string targetUserName, string groupName)
    {
        const string updatedReadOn = "UpdatedReadOn";

        DateTime? updatedOn = await UpdateReadOnAsync(userId, targetUserName, GetCancellationToken());

        await Clients.Group(groupName).SendAsync(updatedReadOn, updatedOn, GetCancellationToken());
    }

    public async Task Create(MessageInDto messageInDto)
    {
        const string newMessageRes = "NewMessageRes";

        ObjectId userId =
            await _tokenService.GetActualUserIdAsync(Context.User?.GetUserIdHashed(), GetCancellationToken())
            ?? throw new HubException("UserId is invalid. Login again.");

        MessageDto messageDto = await _messageRepository.CreateAsync(userId, messageInDto, GetCancellationToken())
                                ?? throw new HubException("MessageDto is null. Message creation failed. Try again.");

        string groupName = GetGroupName(GetUserName(), messageInDto.ReceiverUserName);

        await Clients.Group(groupName).SendAsync(newMessageRes, messageDto, GetCancellationToken());
    }

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

    private async Task AddGroupNameToDb(ObjectId userId, string groupName, CancellationToken cancellationToken)
    {
        if (!await _messageService.AddGroupNameAsync(userId, groupName, cancellationToken))
            throw new HubException("Add groupName to DB failed.");
    }

    private async Task MarkReceiverMessagesAsRead(ObjectId userId, ObjectId receiverId, string groupName)
    {
    }

    private async Task RemoveGroupNameFromDb()
    {
        ObjectId userId =
            await _tokenService.GetActualUserIdAsync(Context.User?.GetUserIdHashed(), GetCancellationToken())
            ?? throw new HubException("UserId is invalid. Login again.");

        // HashSet<string>? groupNames = await _userRepository.GetGroupNamesAsync(userId, GetCancellationToken());

        // if (groupNames is not null && groupNames.Count != 0)
        // {
        //     foreach (string groupName in groupNames)
        //     {
        //         if (!await _userRepository.RemoveGroupNameAsync(userId, groupName, GetCancellationToken()))
        //             throw new HubException("Removing groupName from DB failed.");

        //         await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName, GetCancellationToken());
        //     }
        // }
    }

    private async Task<bool> CheckIsUserInGroupAsync(ObjectId userId, string groupName,
        CancellationToken cancellationToken) =>
        // HashSet<string>? groupNames = await _userRepository.GetGroupNamesAsync(userId, cancellationToken);
        // return groupNames is not null && groupNames.Count > 0 && groupNames.Contains(groupName);
        false;

    private CancellationToken GetCancellationToken() =>
        Context.GetHttpContext()?.RequestAborted
        ?? throw new HubException("CancellationToken is null but cannot be.");

    // private async Task UpdateReadOnAsync(ObjectId userId, string groupName, )
    // {


    //     if (Clients.Equals(groupName)) // Set value for ReadOn in parties are both chatting
    //         messageDto.ReadOn = await UpdateReadOnAsync(userId, messageInDto.ReceiverUserName, cancellationToken)
    //             ?? throw new HubException("ReadOn is null even though it's updated here and cannot be null.");
    // }
}