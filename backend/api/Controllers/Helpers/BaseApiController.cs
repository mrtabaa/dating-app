namespace api.Controllers.Helpers;

[ServiceFilter(typeof(LogUserActivity))]
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting(AppVariablesExtensions.SlidingPolicy)]
public class BaseApiController : ControllerBase
{
}