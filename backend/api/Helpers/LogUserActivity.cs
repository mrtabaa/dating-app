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

        string? loggedInUserEmail = resultContext.HttpContext.User.GetUserEmail();

        IUserRepository userRepository = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

        CancellationToken cancellationToken = resultContext.HttpContext.RequestAborted; // access cancellationToken

        if (!string.IsNullOrEmpty(loggedInUserEmail))
        {
            UpdateResult? updateResult = await userRepository.UpdateLastActive(loggedInUserEmail, cancellationToken);

            if (updateResult?.ModifiedCount == 0)
                _logger.LogError("Update lastActive in db failed. Check LogUserActivity.cs");
        }
        else
            _logger.LogError("loggedInUserId is null which is not allowed here.");
    }
}
