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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto?>>> GetFollows([FromQuery] FollowParams followParams, CancellationToken cancellationToken)
    {
        string? loggedInUserEmail = User.GetUserEmail();

        if (string.IsNullOrEmpty(loggedInUserEmail)) return BadRequest("No user is logged-in!");

        PagedList<AppUser>? pagedAppUsers = await _followRepository.GetFollowMembersAsync(loggedInUserEmail, followParams, cancellationToken);

        if (pagedAppUsers is null) return NoContent();

        /*  1- Response only exists in Contoller. So we have to set PaginationHeader here before converting AppUser to UserDto.
                If we convert AppUser before here, we'll lose PagedList's pagination values, e.g. CurrentPage, PageSize, etc.
        */
        Response.AddPaginationHeader(new PaginationHeader(pagedAppUsers.CurrentPage, pagedAppUsers.PageSize, pagedAppUsers.TotalCount, pagedAppUsers.TotalPages));

        /*  2- PagedList<T> has to be AppUser first to retrieve data from DB and set pagination values. 
                After that step we can convert AppUser to MemberDto in here (NOT in the UserRepository) */
        List<MemberDto?> memberDtos = [];

        foreach (AppUser pagedAppUser in pagedAppUsers)
        {
            memberDtos.Add(Mappers.ConvertAppUserToMemberDto(pagedAppUser));
        }

        return memberDtos;
    }
}
