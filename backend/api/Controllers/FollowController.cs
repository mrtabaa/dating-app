namespace api.Controllers;

[Authorize]
public class FollowController(IFollowRepository _followRepository, IUserRepository _userRepository) : BaseApiController
{
    [HttpPost("{targetMemberEmail}")]
    public async Task<ActionResult> AddFollow(string targetMemberEmail, CancellationToken cancellationToken)
    {
        string? loggedInUserEmail = User.GetUserEmail();

        if (!string.IsNullOrEmpty(loggedInUserEmail))
        {
            if (loggedInUserEmail == targetMemberEmail)
                return BadRequest("Following yourself is great but is not stored!");

            FolowStatus followStatus = await _followRepository.AddFollowAsync(loggedInUserEmail, targetMemberEmail, cancellationToken);
            if (followStatus.IsSuccess)
                return Ok();

            if (followStatus.IsAlreadyFollowed)
            {
                string? knownAs = await _userRepository.GetKnownAsByEmailAsync(targetMemberEmail, cancellationToken);
                if (knownAs is not null)
                    return BadRequest($"{knownAs} is already followed.");
            }

            if (followStatus.IsTargetMemberEmailWrong)
                return BadRequest("Wrong target email is given.");

            return BadRequest("Following the member failed. Try agian.");
        }

        return BadRequest("Operation failed. Contact the admin.");
    }

    [HttpGet()]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetFollows(FollowParams followParams, CancellationToken cancellationToken)
    {
        string? loggedInUserEmail = User.GetUserEmail();

        if (string.IsNullOrEmpty(loggedInUserEmail)) return BadRequest("No user is logged-in!");

        IEnumerable<MemberDto> memberDtos = await _followRepository.GetFollowMembersAsync(loggedInUserEmail, followParams, cancellationToken);

        if (!memberDtos.Any()) return NoContent();

        return Ok(memberDtos);
    }
}
