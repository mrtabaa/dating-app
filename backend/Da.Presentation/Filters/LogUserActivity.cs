using Microsoft.AspNetCore.Mvc.Filters;

namespace Da.Presentation.Filters;

public class LogUserActivity(ILogger<LogUserActivity> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ActionExecutedContext resultContext = await next(); // After api's processing is done. 
        // Use 'context' instead of 'next' for before api's processing.

        // return if User is not authenticated
        if (resultContext.HttpContext.User.Identity is not null &&
            !resultContext.HttpContext.User.Identity.IsAuthenticated) return;

        string loggedInUserIdHashed = resultContext.HttpContext.User.GetUserIdHashed()
                                      ?? throw new ArgumentNullException(
                                          nameof(loggedInUserIdHashed), "Parameter cannot be null"
                                      );

        var accountService = resultContext.HttpContext.RequestServices.GetRequiredService<IAccountService>();

        CancellationToken cancellationToken = resultContext.HttpContext.RequestAborted; // access cancellationToken

        OperationResult result = await accountService.UpdateLastActive(
            loggedInUserIdHashed, cancellationToken
        );

        if (!result.IsSuccess)
            logger.LogError("Update lastActive in db failed. Check LogUserActivity.cs");
    }
}