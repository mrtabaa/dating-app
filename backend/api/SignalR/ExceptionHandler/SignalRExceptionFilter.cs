namespace api.Middleware;

public class SignalRExceptionFilter : IHubFilter
{
    private readonly ILogger<SignalRExceptionFilter> _logger;
    private readonly IMongoCollection<ApiException> _collection;

    public SignalRExceptionFilter(ILogger<SignalRExceptionFilter> logger, IMongoClient client, IMyMongoDbSettings dbSettings)
    {
        _logger = logger;
        IMongoDatabase dbName = client.GetDatabase(dbSettings.DatabaseName) ?? throw new ArgumentNullException(nameof(dbName));
        _collection = dbName.GetCollection<ApiException>(AppVariablesExtensions.collectionExceptionLogs);
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
            ApiException apiException = new ApiException
            {
                Id = ObjectId.Empty,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = ex.Message,
                Details = ex.StackTrace?.ToString(),
                Time = DateTime.Now
            };
            await _collection.InsertOneAsync(apiException);

            // Optionally log with ILogger
            _logger.LogError(ex, ex.Message);

            throw; // Optionally throw or handle the exception
        }
    }
}

