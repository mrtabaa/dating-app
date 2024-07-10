namespace api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessageController(
        ITokenService _tokenService, IMessageRepository _messageRepository,
        IMemberRepository _memberRepository, IPhotoService _photoService
    ) : BaseApiController
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> Get([FromQuery] MessageParams messageParams, CancellationToken cancellationToken)
    {
        List<MessageDto> messageDtos = [];

        ObjectId? userId = await _tokenService.GetActualUserIdAsync(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return Unauthorized("User id is invalid. Login again.");

        PagedList<Message>? pagedMessages;

        if (messageParams.Predicate == MessagePredicate.Thread)
            pagedMessages = await _messageRepository.GetAsync(userId.Value, messageParams, cancellationToken);
        else
        {
            pagedMessages = await _messageRepository.GetThreadAsync(userId.Value, messageParams, cancellationToken);
            if (pagedMessages is null) return NotFound("Target user was not found.");
        }

        Response.AddPaginationHeader(new PaginationHeader(
            pagedMessages.CurrentPage, pagedMessages.PageSize, pagedMessages.TotalItemsCount, pagedMessages.TotalPages));

        if (pagedMessages.Count == 0) return NoContent();

        IEnumerable<AppUser> userOrTargets = await GetAllMembers(pagedMessages, cancellationToken);

        AppUser? userOrTarget;
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
