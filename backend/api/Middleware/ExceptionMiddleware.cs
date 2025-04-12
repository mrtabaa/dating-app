namespace api.Middleware;

public class ExceptionMiddleware
{
    private readonly IMongoCollection<ApiException> _collection;
    private readonly IHostEnvironment _env;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(
        RequestDelegate next,
        IHostEnvironment env,
        IMongoClient client,
        IMyMongoDbSettings dbSettings,
        ILogger<ExceptionMiddleware> logger
    )
    {
        _next = next;
        _env = env;
        _logger = logger;

        IMongoDatabase dbName = client.GetDatabase(dbSettings.DatabaseName)
                                ?? throw new ArgumentNullException(nameof(dbName));
        _collection = dbName.GetCollection<ApiException>(AppVariablesExtensions.CollectionExceptionLogs);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            ApiException response = new()
            {
                Id = ObjectId.Empty,
                StatusCode = context.Response.StatusCode,
                Message = ex.Message,
                Details = ex.StackTrace,
                Time = DateTime.Now
            };

            await _collection.InsertOneAsync(response);

            if (_env.IsProduction())
                response.Details = "Internal Server Error.";

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            string json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }
    }
}