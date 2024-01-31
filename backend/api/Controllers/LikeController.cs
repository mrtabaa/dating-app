namespace api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LikeController(ILikesRepository _likesRepository) : BaseApiController
{
    [HttpPost("{targetMemberId}")]
    public async Task<ActionResult> AddLike(string targetMemberId, CancellationToken cancellationToken)
    {
        string? loggedInUserId = User.GetUserId();

        if (!string.IsNullOrEmpty(loggedInUserId))
        {
            bool success = await _likesRepository.AddLikeAsync(loggedInUserId, targetMemberId, cancellationToken);
            if (success)
                return Ok();

            return BadRequest("Liking the member failed. Try agian.");
        }

        return BadRequest("The user is not logged-in or ID has failed. Contact the admin.");
    }
}
