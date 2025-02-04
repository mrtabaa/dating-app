namespace api.Controllers.Helpers;

[ServiceFilter(typeof(LogUserActivity))]
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting(AppVariablesExtensions.SlidingWindowPolicy)]
public class BaseApiController : ControllerBase
{

}