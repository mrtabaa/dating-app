namespace api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessageController(
    ITokenService tokenService,
    IMessageRepository messageRepository,
    IMemberRepository memberRepository,
    IPhotoService photoService
) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto?>> Create(MessageInDto messageInDto, CancellationToken cancellationToken)
    {
        ObjectId? userId = await tokenService.GetActualUserIdAsync(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return Unauthorized("User id is invalid. Login again.");

        MessageDto? messageDto = await messageRepository.CreateAsync(userId.Value, messageInDto, cancellationToken);

        return messageDto is null
            ? BadRequest("Sending message failed. Try again or contact the support.")
            : messageDto;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> Get([FromQuery] MessageParams messageParams,
        CancellationToken cancellationToken)
    {
        List<MessageDto> messageDtos = [];

        ObjectId? userId = await tokenService.GetActualUserIdAsync(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return Unauthorized("User id is invalid. Login again.");

        PagedList<Message>? pagedMessages;

        if (messageParams.Predicate == MessagePredicate.Thread)
        {
            pagedMessages = await messageRepository.GetThreadAsync(userId.Value, messageParams, cancellationToken);
            if (pagedMessages is null) return NotFound("Target user was not found.");
        }
        else
            pagedMessages = await messageRepository.GetAsync(userId.Value, messageParams, cancellationToken);

        Response.AddPaginationHeader(new PaginationHeader(
            pagedMessages.CurrentPage, pagedMessages.PageSize, pagedMessages.TotalItemsCount, pagedMessages.TotalPages));

        if (pagedMessages.Count == 0) return NoContent();

        IEnumerable<AppUser> userOrTargets = await GetAllMembers(pagedMessages, cancellationToken);

        foreach (Message message in pagedMessages)
        {
            AppUser? userOrTarget = messageParams.Predicate == MessagePredicate.Sent
                ? userOrTargets.FirstOrDefault(member => member.Id == message.ReceiverId) // To set receiver photo instead of sender's photo 
                : userOrTargets.FirstOrDefault(member => member.Id == message.SenderId);

            if (userOrTarget is null) continue;

            // Convert all targetMember profile photo to blob Sas format
            string? profilePhotoUrl = userOrTarget.Photos.FirstOrDefault(photo => photo.IsMain)?.Url165;

            string? profilePhotoSasUrl = photoService.ConvertPhotoUrlToBlobLinkWithSas(profilePhotoUrl);

            messageDtos.Add(Mappers.ConvertMessageToMessageDto(message, userOrTarget, profilePhotoSasUrl));
        }

        return messageDtos;
    }

    private async Task<IEnumerable<AppUser>> GetAllMembers(PagedList<Message> pagedMessages, CancellationToken cancellationToken)
    {
        // Get all Ids in the messages (sender & receiver)
        IEnumerable<ObjectId> allIds = pagedMessages.Select(message => message.SenderId) // Get senders' Ids
            .Concat(pagedMessages.Select(message => message.ReceiverId)) // Get receivers' Ids and merge with senders' Ids
            .Distinct(); // Eliminates duplicate Ids

        return await memberRepository.GetAppUsersByIdsAsync(allIds, cancellationToken);
    }
}