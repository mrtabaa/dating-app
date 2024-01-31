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
            if (loggedInUserId == targetMemberId)
                return BadRequest("Liking yourself is great but is not stored!");

            LikeStatus likeStatus = await _likesRepository.AddLikeAsync(loggedInUserId, targetMemberId, cancellationToken);
            if (likeStatus.IsSuccess)
                return Ok();

            if (likeStatus.IsAlreadyLiked)
                return BadRequest("The user is already liked.");

            return BadRequest("Liking the member failed. Try agian.");
        }

        return BadRequest("Operation failed. Contact the admin.");
    }
}
