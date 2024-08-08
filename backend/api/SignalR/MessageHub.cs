using System.Web;

namespace api.SignalR;

public class MessageHub(
    IMessageRepository _messageRepository, ITokenService _tokenService,
    IMemberRepository _memberRepository, IPhotoService _photoService) : Hub
{
    public override async Task OnConnectedAsync()
    {
        const string MessageThread = "MessageThread";

        HttpContext? httpContext = Context.GetHttpContext();

        string? messageParamsString = httpContext?.Request.QueryString.Value; // TODO check the value in debugger

        if (string.IsNullOrEmpty(messageParamsString)) return;

        MessageParams messageParams = ParseQueryStringToMessageParams(messageParamsString);

        string? caller = Context.User?.GetUserName();

        if (!(httpContext is null || string.IsNullOrEmpty(caller) || string.IsNullOrEmpty(messageParams.TargetUserName)))
        {
            string groupName = GetGroupName(caller, messageParams.TargetUserName);

            CancellationToken cancellationToken = httpContext.RequestAborted;

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName, cancellationToken);

            MessagesWithPaginationDto messagesWithPaginationDto = await GetMessageDtos(httpContext.Response, messageParams, cancellationToken);

            await Clients.Group(groupName).SendAsync(MessageThread, messagesWithPaginationDto);
        }
    }

    private MessageParams ParseQueryStringToMessageParams(string queryString)
    {
        var queryCollection = HttpUtility.ParseQueryString(queryString);
        var messageParams = new MessageParams();

        if (queryCollection["pageNumber"] is not null)
        {
            messageParams.PageNumber = int.TryParse(queryCollection["pageNumber"], out int pageNumber) ? pageNumber : 1;
        }

        if (queryCollection["pageSize"] is not null)
        {
            messageParams.PageSize = int.TryParse(queryCollection["pageSize"], out int pageSize) ? pageSize : 25;
        }

        if (queryCollection["predicate"] is not null)
        {
            messageParams.Predicate = Enum.TryParse(queryCollection["predicate"], out MessagePredicate predicate) ? predicate : MessagePredicate.Thread;
        }

        messageParams.TargetUserName = queryCollection["targetUserName"] ?? string.Empty;

        return messageParams;
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

    private async Task<MessagesWithPaginationDto> GetMessageDtos(HttpResponse httpResponse, MessageParams messageParams, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(Context.User?.GetUserIdHashed(), cancellationToken)
            ?? throw new HubException("User id is invalid. Login again.");

        PagedList<Message>? pagedMessages;

        if (messageParams.Predicate == MessagePredicate.Thread)
        {
            pagedMessages = await _messageRepository.GetThreadAsync(userId.Value, messageParams, cancellationToken);
            if (pagedMessages is null) throw new HubException("Target user was not found.");
        }
        else
            pagedMessages = await _messageRepository.GetAsync(userId.Value, messageParams, cancellationToken);

        if (pagedMessages.Count == 0) throw new HubException("No content.");

        IEnumerable<AppUser> userOrTargets = await GetAllMembers(pagedMessages, cancellationToken);

        AppUser? userOrTarget;
        List<MessageDto> messageDtos = [];

        foreach (var message in pagedMessages)
        {
            userOrTarget = userOrTargets.FirstOrDefault(member => member.Id == message.SenderId);

            if (userOrTarget is not null)
            {
                // Convert all targetMember profile photo to blob Sas format
                string? profilePhotoUrl = userOrTarget.Photos.FirstOrDefault(photo => photo.IsMain)?.Url_165;

                string? profilePhotoSasUrl = _photoService.ConvertPhotoUrlToBlobLinkWithSas(profilePhotoUrl);

                messageDtos.Add(Mappers.ConvertMessageToMessageDto(message, userOrTarget, profilePhotoSasUrl));
            }
        }

        MessagesWithPaginationDto messageWithPaginationDto = new()
        {
            PaginationHeader = new PaginationHeader(
            pagedMessages.CurrentPage, pagedMessages.PageSize, pagedMessages.TotalItemsCount, pagedMessages.TotalPages),
            MessageDtos = messageDtos
        };

        return messageWithPaginationDto;
    }

    private async Task<IEnumerable<AppUser>> GetAllMembers(PagedList<Message> pagedMessages, CancellationToken cancellationToken)
    {
        // Get all Ids in the messages (sender & receiver)
        IEnumerable<ObjectId> allIds = pagedMessages.Select(message => message.SenderId) // Get senders' Ids
            .Concat(pagedMessages.Select(message => message.RecieverId)) // Get receivers' Ids and merge with senders' Ids
            .Distinct(); // Eliminates duplicate Ids

        return await _memberRepository.GetMembersByIdsAsync(allIds, cancellationToken);
    }
}
