using Microsoft.AspNetCore.Mvc.Filters;

namespace api.Helpers;

public class LogUserActivity(ILogger<LogUserActivity> _logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next(); // After api's processing is done. 
        // Use 'context' instead of 'next' for before api's processing.

        // return if User is not authenticated
        if (resultContext.HttpContext.User.Identity is not null && !resultContext.HttpContext.User.Identity.IsAuthenticated) return;

        string? loggedInUserIdHashed = resultContext.HttpContext.User.GetUserIdHashed();

        IAccountRepository accountRepository = resultContext.HttpContext.RequestServices.GetRequiredService<IAccountRepository>();

        CancellationToken cancellationToken = resultContext.HttpContext.RequestAborted; // access cancellationToken

        if (string.IsNullOrEmpty(loggedInUserIdHashed))
        {
            _ = loggedInUserIdHashed ?? throw new ArgumentException("Parameter cannot be null", nameof(loggedInUserIdHashed));
            return;
        }

        UpdateResult? updateResult = await accountRepository.UpdateLastActive(loggedInUserIdHashed, cancellationToken);

        if (updateResult?.ModifiedCount == 0)
            _logger.LogError("Update lastActive in db failed. Check LogUserActivity.cs");
    }
}
