namespace api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessageController(ITokenService _tokenService, IMessageRepository _messageRepository) : BaseApiController
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

    // [HttpGet]
    // public async Task<ActionResult<IEnumerable<MessageDto>>> GetUserMessages([FromQuery] PaginationParams pageParams, CancellationToken cancellationToken)
    // {
    //     ObjectId? userId = await _tokenService.GetActualUserIdAsync(User.GetUserIdHashed(), cancellationToken);
    //     if (userId is null)
    //         return Unauthorized("User id is invalid. Login again.");

    //     PagedList<Message> pagedMessages = await _messageRepository.GetUserMessagesAsync(userId.Value, pageParams, cancellationToken);

    //     Response.AddPaginationHeader(new PaginationHeader(
    //         pagedMessages.CurrentPage, pagedMessages.PageSize, pagedMessages.TotalItemsCount, pagedMessages.TotalPages));

    //     if (pagedMessages.Count == 0) return NoContent();

    //     List<MessageDto> messageDtos = [];

    //     foreach (var message in pagedMessages)
    //     {
    //         messageDtos.Add(
    //             Mappers.ConvertMessageToMessageDto(message, )
    //         );
    //     }

    //     return messageDtos;
    // }
}
