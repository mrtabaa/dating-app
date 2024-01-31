namespace api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LikeController(ILikesRepository _likesRepository) : BaseApiController
{
    [HttpPost("{targetMemberId}")]
    public async Task<ActionResult> AddLike(string targetMemberId, CancellationToken cancellationToken)
    {
        if (!ValidateMongoDbId(targetMemberId))
            return BadRequest("Invalid id is given.");

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

    [HttpGet("{predicate}")]
    public async Task<ActionResult<IEnumerable<Like>?>> GetLikes(string predicate, CancellationToken cancellationToken)
    {
        string? loggedInUserId = User.GetUserId();

        if (string.IsNullOrEmpty(loggedInUserId)) return BadRequest("No user is logged-in!");

        List<Like>? likes = await _likesRepository.GetLikedMembersAsync(loggedInUserId, predicate, cancellationToken);

        if (likes?.Count == 0) return NoContent();

        return likes;
    }

    private static bool ValidateMongoDbId(string targetMemberId)
    {
        // TODO Validate all mongo IDs before any query to prevent exceptions
        return ObjectId.TryParse(targetMemberId, out ObjectId objectId);
    }
}
