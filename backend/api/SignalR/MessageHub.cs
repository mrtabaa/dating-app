namespace api.SignalR;

[Authorize]
public class MessageHub(
    IMessageRepository messageRepository,
    IMessageService messageService,
    IUserRepository userRepository,
    ITokenService tokenService
) : Hub
{
    public async Task JoinGroup(string? targetUserName)
    {
        if (string.IsNullOrEmpty(targetUserName))
            throw new HubException("targetUserName cannot be null or empty.");

        ObjectId? userId = await GetUserId();
        if (userId is null)
        {
            await Clients.Caller.SendAsync(SignalRMessages.SendingError, "Unauthorized.", GetCancellationToken());
            return;
        }

        string groupName = GetGroupName(
            await GetUserName() ?? throw new ArgumentNullException(nameof(GetGroupName))
            , targetUserName
        );

        await AddGroupNameToDb(userId.Value, groupName);

        // TODO: Handle exceptions with middleware
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await UpdateReadOnAsync(userId.Value, targetUserName, groupName, MessageOperation.Join);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it appropriately
            throw new HubException("An error occurred while joining the group.", ex);
        }
    }

    private async Task<DateTime> UpdateReadOnAsync(
        ObjectId userId, string targetUserName, string groupName, MessageOperation operation
    )
    {
        OperationResult<ObjectId> receiverUserIdResult = await userRepository.GetIdByUserNameAsync(
            targetUserName.ToUpper(), GetCancellationToken()
        );
        if (!receiverUserIdResult.IsSuccess)
            throw new HubException("OtherUserId is invalid.");

        DateTime updatedReadOn = operation switch
        {
            MessageOperation.Join => await messageRepository.UpdateReadOnAsync(
                receiverUserIdResult.Result, userId, GetCancellationToken()
            ),
            MessageOperation.Create => await messageRepository.UpdateReadOnAsync(
                userId, receiverUserIdResult.Result, GetCancellationToken()
            ),
            _ => throw new InvalidOperationException(
                $"Invalid operation: {operation}"
            ) // Throw an exception for any invalid operation
        };

        await Clients.Group(groupName).SendAsync(SignalRMessages.UpdatedReadOn, updatedReadOn, GetCancellationToken());

        return updatedReadOn;
    }

    public async Task Create(MessageInDto messageInDto)
    {
        string? userIdHashed = Context.User?.GetUserIdHashed();
        if (string.IsNullOrEmpty(userIdHashed))
        {
            await Clients.Caller.SendAsync(SignalRMessages.SendingError, "Unauthorized.", GetCancellationToken());
            return;
        }

        #region RateLimiting

        if (RateLimitingHubs.RateLimitSliding(userIdHashed, Clients, GetCancellationToken()))
            return;

        // Release semaphore after try{} in finally{}
        SemaphoreSlim? semaphore = await RateLimitingHubs.RateLimitConcurrentAsync(
            userIdHashed, Clients, GetCancellationToken()
        );
        if (semaphore is null)
            return;

        #endregion

        try
        {
            messageInDto.Content = messageInDto.Content.Trim();

            if (ValidateContent(messageInDto.Content))
                return;

            ObjectId? userId = await GetUserId();

            if (userId is null)
            {
                await Clients.Caller.SendAsync(SignalRMessages.SendingError, "Unauthorized.", GetCancellationToken());
                return;
            }

            MessageDto messageDto = await messageRepository.CreateAsync(
                                        userId.Value, messageInDto, GetCancellationToken()
                                    )
                                    ?? throw new HubException(
                                        "MessageDto is null. Message creation failed. Try again."
                                    );

            _ = messageDto.UserOrTargetUserName
                ?? throw new HubException("UserOrTargetUserName is null. Message creation failed. Try again.");

            string groupName = GetGroupName(
                await GetUserName() ?? throw new ArgumentNullException(nameof(GetGroupName))
                , messageInDto.ReceiverUserName
            );

            OperationResult<ObjectId> receiverUserIdResult = await userRepository.GetIdByUserNameAsync(
                messageInDto.ReceiverUserName, GetCancellationToken()
            );
            if (!receiverUserIdResult.IsSuccess)
                throw new HubException("OtherUserId is invalid.");

            // Update and get ReadOn if target member is in a group. Also update messageDto.ReadOn 
            if (await messageService.CheckIsMemberIsInGroupAsync(
                    receiverUserIdResult.Result, groupName, GetCancellationToken()
                ))
            {
                messageDto.ReadOn = await UpdateReadOnAsync(
                    userId.Value, messageInDto.ReceiverUserName, groupName, MessageOperation.Create
                );
            }

            await Clients.Group(groupName).SendAsync(SignalRMessages.NewMessageRes, messageDto, GetCancellationToken());
        }
        finally
        {
            semaphore.Release();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        ObjectId? userId = await GetUserId();
        if (userId is null)
        {
            await Clients.Caller.SendAsync(SignalRMessages.SendingError, "Unauthorized.", GetCancellationToken());
            return;
        }

        MessageGroup messageGroup = await messageService.GetMessageGroupAsync(
                                        userId.Value, Context.ConnectionId, GetCancellationToken()
                                    )
                                    ?? throw new HubException(
                                        "MessageGroup is not found. DB MessageGroup removal will fail."
                                    );

        if (!await messageService.RemoveMessageGroupAsync(userId.Value, messageGroup))
            throw new HubException("Removing MessageGroup from DB failed.");

        await base.OnDisconnectedAsync(exception);
    }

    private async Task AddGroupNameToDb(ObjectId userId, string groupName)
    {
        MessageGroup messageGroup = new(
            Context.ConnectionId,
            groupName
        );

        if (!await messageService.AddMessageGroupAsync(userId, messageGroup, GetCancellationToken()))
            throw new HubException("Add messageGroup to DB failed.");
    }

    private async Task<string?> GetUserName()
    {
        string? userIdHashed = Context.User?.GetUserIdHashed();
        if (string.IsNullOrEmpty(userIdHashed))
        {
            await Clients.Caller.SendAsync(SignalRMessages.SendingError, "Unauthorized.", GetCancellationToken());
            return null;
        }

        return await userRepository.GetUserNameByIdentifierHashAsync(userIdHashed, GetCancellationToken())
               ?? throw new HubException("UserName is invalid. Login again.");
    }

    private async Task<ObjectId?> GetUserId() =>
        await tokenService.GetActualUserIdAsync(
            Context.User?.GetUserIdHashed(), GetCancellationToken()
        );

    private static bool ValidateContent(string content)
    {
        string trimmedContent = content.Trim(); // Remove space/enter
        return string.IsNullOrEmpty(trimmedContent) || trimmedContent.Length > 500;
    }

    private static string GetGroupName(string caller, string other)
    {
        string otherUpper = other.ToUpper();
        bool stringCompare = string.CompareOrdinal(caller, otherUpper) < 0;

        return stringCompare ? $"{caller}-{otherUpper}" : $"{otherUpper}-{caller}";
    }

    private CancellationToken GetCancellationToken() =>
        Context.GetHttpContext()?.RequestAborted
        ?? throw new HubException("CancellationToken is null but cannot be.");
}

public enum MessageOperation
{
    Join,
    Create
}