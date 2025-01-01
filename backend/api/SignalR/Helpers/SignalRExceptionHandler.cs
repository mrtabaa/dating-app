namespace api.SignalR.Helpers;

public class SignalRExceptionHandler : IHubFilter
{
    private readonly IMongoCollection<ApiException> _collection;
    private readonly ILogger<SignalRExceptionHandler> _logger;

    public SignalRExceptionHandler(ILogger<SignalRExceptionHandler> logger, IMongoClient client, IMyMongoDbSettings dbSettings)
    {
        _logger = logger;
        IMongoDatabase dbName = client.GetDatabase(dbSettings.DatabaseName) ?? throw new ArgumentNullException(nameof(dbName));
        _collection = dbName.GetCollection<ApiException>(AppVariablesExtensions.CollectionExceptionLogs);
    }

    public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
    {
        try
        {
            return await next(invocationContext);
        }
        catch (Exception ex)
        {
            // Log to MongoDB
            var apiException = new ApiException
            {
                Id = ObjectId.Empty,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = ex.Message,
                Details = ex.StackTrace,
                Time = DateTime.Now
            };
            await _collection.InsertOneAsync(apiException);

            // Optionally log with ILogger
            _logger.LogError(ex, ex.Message);

            throw; // Optionally throw or handle the exception
        }
    }
}