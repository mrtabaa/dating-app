namespace api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessageController(
    ITokenService _tokenService, IMessageRepository _messageRepository,
    IUserRepository _userRepository, IMemberRepository _memberRepository) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult> Create(MessageInDto messageInDto, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return Unauthorized("User id is invalid. Login again.");

        MessageStatus mS = await _messageRepository.CreateAsync(userId.Value, messageInDto, cancellationToken);

        return mS.IsSuccess
        ? Ok("Message sent.")
        : mS.IsReceiverNotFound
        ? NotFound($"The target member {messageInDto.ReceiverUserName} is not found.")
        : BadRequest("Sending message faild. Try again or contact the support.");
    }

    [HttpGet("inbox")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetInboxMessages([FromQuery] PaginationParams pageParams, CancellationToken cancellationToken)
    {
        List<MessageDto> messageDtos = [];

        ObjectId? userId = await _tokenService.GetActualUserIdAsync(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return Unauthorized("User id is invalid. Login again.");

        PagedList<Message> pagedMessages = await _messageRepository.GetInboxMessagesAsync(userId.Value, pageParams, cancellationToken);

        Response.AddPaginationHeader(new PaginationHeader(
            pagedMessages.CurrentPage, pagedMessages.PageSize, pagedMessages.TotalItemsCount, pagedMessages.TotalPages));

        if (pagedMessages.Count == 0) return NoContent();

        string? loggedInUserName = await _userRepository.GetUserNameById(userId.Value, cancellationToken);

        if (loggedInUserName is null)
            return Unauthorized("Logged in user's Username is invalid. Login again.");

        IEnumerable<AppUser> targetMembers = await GetAllMembers(pagedMessages, cancellationToken);

        foreach (var message in pagedMessages)
        {

            if (message.SenderId == userId) // sender is loggedInUser
            {
                AppUser? targetMember = targetMembers.FirstOrDefault(member => member.Id == message.RecieverId);

                messageDtos.Add(
                    new MessageDto(
                        Id: message.Id.ToString(),
                        Content: message.Content,
                        SenderUserName: loggedInUserName,
                        ReceiverUserName: targetMember?.UserName,
                        TargetUserProfilePhoto: targetMember?.Photos.FirstOrDefault(ph => ph.IsMain)?.Url_165,
                        ReadOn: message.ReadOn,
                        SentOn: message.SentOn
                    )
                );
            }
            else
            {
                AppUser? targetMember = targetMembers.FirstOrDefault(member => member.Id == message.SenderId);

                messageDtos.Add(
                    new MessageDto( // sender is targetMember
                        Id: message.Id.ToString(),
                        Content: message.Content,
                        SenderUserName: targetMember?.UserName,
                        ReceiverUserName: loggedInUserName,
                        TargetUserProfilePhoto: targetMember?.Photos.FirstOrDefault(ph => ph.IsMain)?.Url_165,
                        ReadOn: message.ReadOn,
                        SentOn: message.SentOn
                    )
                );
            }
        }

        return messageDtos;
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
