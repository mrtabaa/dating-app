using Microsoft.AspNetCore.Antiforgery;

namespace api.SignalR.Helpers;

public class CsrfHubFilter(ILogger<CsrfHubFilter> logger) : IHubFilter
{
    private readonly string _cookieName = "XSRF-TOKEN";
    private readonly string _headerName = "X-XSRF-TOKEN";

    public async Task OnConnectedAsync(HubLifetimeContext context, Func<Task> next)
    {
        HubCallerContext hubCallerContext = context.Context;
        HttpContext? httpContext = hubCallerContext.GetHttpContext();

        if (httpContext != null)
        {
            string? requestToken = httpContext.Request.Headers[_headerName].FirstOrDefault();
            string? cookieToken = httpContext.Request.Cookies[_cookieName];

            if (string.IsNullOrEmpty(requestToken) || string.IsNullOrEmpty(cookieToken))
            {
                logger.LogWarning("CSRF token missing during SignalR connection.");
                httpContext.Abort(); // Disconnect the client
                return;
            }

            try
            {
                var antiforgery = httpContext.RequestServices.GetRequiredService<IAntiforgery>();
                await antiforgery.ValidateRequestAsync(httpContext);
                logger.LogInformation("CSRF token validated for SignalR connection.");
            }
            catch (AntiforgeryValidationException ex)
            {
                logger.LogError(ex, "CSRF token validation failed for SignalR connection.");
                httpContext.Abort(); // Disconnect the client
                return;
            }
        }
        else
        {
            logger.LogWarning("HttpContext is null during SignalR connection.");
            httpContext?.Abort();
            return;
        }

        await next(); // Continue with the connection
    }

    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception exception, Func<Task> next) => next();

    public async Task InvokeMethodAsync(HubInvocationContext context, Func<Task<object>> next)
    {
        HttpContext? httpContext = context.Context.GetHttpContext();
        if (httpContext != null &&
            context.HubMethodName != "getOnlineUsers") // Example: Don't validate on read-only methods
        {
            string? requestToken = httpContext.Request.Headers[_headerName].FirstOrDefault();
            string? cookieToken = httpContext.Request.Cookies[_cookieName];

            if (string.IsNullOrEmpty(requestToken) || string.IsNullOrEmpty(cookieToken))
            {
                logger.LogWarning($"CSRF token missing for SignalR method: {context.HubMethodName}");
                throw new HubException("CSRF token is missing.");
            }

            try
            {
                var antiforgery = httpContext.RequestServices.GetRequiredService<IAntiforgery>();
                await antiforgery.ValidateRequestAsync(httpContext);
                logger.LogInformation($"CSRF token validated for SignalR method: {context.HubMethodName}");
            }
            catch (AntiforgeryValidationException ex)
            {
                logger.LogError(ex, $"CSRF token validation failed for SignalR method: {context.HubMethodName}");
                throw new HubException("CSRF token validation failed.");
            }
        }

        await next();
    }
}