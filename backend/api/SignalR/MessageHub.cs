using System.Text.RegularExpressions;

namespace api.SignalR;

[Authorize]
public class MessageHub : Hub
// (IMessageRepository _messageRepository, ITokenService _tokenService, ILogger<MessageHub> _logger) : Hub
{
    private readonly IMongoCollection<ApiException> _collection;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IMessageRepository _messageRepository;
    private readonly ITokenService _tokenService;

    public MessageHub(
        IMongoClient client, IMyMongoDbSettings dbSettings,
        ITokenService tokenService, IMessageRepository messageRepository,
        ILogger<ExceptionMiddleware> logger
    )
    {
        IMongoDatabase? dbName = client.GetDatabase(dbSettings.DatabaseName) ?? throw new ArgumentNullException(nameof(dbName));
        _collection = dbName.GetCollection<ApiException>(AppVariablesExtensions.collectionExceptionLogs);

        _tokenService = tokenService;
        _messageRepository = messageRepository;
        _logger = logger;
    }

    public async Task JoinGroup(string targetUserName)
    {
        string groupName = GetGroupName(GetUserName(), targetUserName.ToUpper());

        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
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

        SaveLog(1, httpContext, null, messageInDto);

        CancellationToken cancellationToken = httpContext.RequestAborted;

        ObjectId userId = await _tokenService.GetActualUserIdAsync(Context.User?.GetUserIdHashed(), cancellationToken)
            ?? throw new HubException("User id is invalid. Login again.");

        SaveLog(2, httpContext, userId, messageInDto);

        MessageDto messageDto = await _messageRepository.CreateAsync(userId, messageInDto, cancellationToken)
            ?? throw new HubException("Message creation failed. Try again.");

        SaveLog(3, httpContext, userId, messageInDto, messageDto);

        string groupName = GetGroupName(GetUserName(), messageInDto.ReceiverUserName.ToUpper());

        SaveLog(4, httpContext, userId, messageInDto, messageDto, groupName);

        await Clients.Group(groupName).SendAsync(NewMessageRes, messageDto, cancellationToken);
    }

    private static string GetGroupName(string caller, string other)
    {
        bool stringCompare = string.CompareOrdinal(caller, other) < 0;

        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    private string GetUserName() =>
        Context.User?.GetUserName() ?? throw new HubException("UserName is invalid. Login again.");

    private async void SaveLog(int number, HttpContext context, ObjectId? userId = null, MessageInDto? messageInDto = null, MessageDto? messageDto = null, string? groupName = null)
    {
        ApiException response = new()
        {
            Id = ObjectId.Empty,
            StatusCode = context.Response.StatusCode,
            Number = number,
            UserId = userId,
            Message = groupName,
            MessageInDto = messageInDto,
            MessageDto = messageDto,
            Time = DateTime.Now
        };

        await _collection.InsertOneAsync(response);
    }
}
