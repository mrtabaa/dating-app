namespace api.Middleware;

public sealed class AuthenticatedUserIdGuardMiddleware(RequestDelegate next, ILogger<AuthenticatedUserIdGuardMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<AuthenticatedUserIdGuardMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && string.IsNullOrWhiteSpace(context.User.GetUserIdHashed()))
        {
            _logger.LogError("Authenticated user is missing user id.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Authenticated user is missing user id.");
            return;
        }

        await _next(context);
    }
}
