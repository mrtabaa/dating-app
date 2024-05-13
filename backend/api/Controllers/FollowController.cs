namespace api.Controllers;

[Authorize]
public class FollowController(IFollowRepository _followRepository, ITokenService _tokenService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetFollows([FromQuery] FollowParams followParams, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserId(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return BadRequest("User id is invalid. Login again.");

        PagedList<AppUser>? pagedAppUsers = await _followRepository.GetFollowMembersAsync(userId.Value, followParams, cancellationToken);

        if (pagedAppUsers is null) return BadRequest("Getting members faild");

        if (pagedAppUsers.Count == 0) return NoContent();

        /*  1- Response only exists in Contoller. So we have to set PaginationHeader here before converting AppUser to UserDto.
                If we convert AppUser before here, we'll lose PagedList's pagination values, e.g. CurrentPage, PageSize, etc.
        */
        Response.AddPaginationHeader(new PaginationHeader(pagedAppUsers.CurrentPage, pagedAppUsers.PageSize, pagedAppUsers.TotalItemsCount, pagedAppUsers.TotalPages));

        /*  2- PagedList<T> has to be AppUser first to retrieve data from DB and set pagination values. 
                After that step we can convert AppUser to MemberDto in here (NOT in the UserRepository) */
        List<MemberDto> memberDtos = [];

        foreach (AppUser pagedAppUser in pagedAppUsers)
        {
            if (await _followRepository.CheckIsFollowing(userId.Value, pagedAppUser, cancellationToken))
            {
                memberDtos.Add(Mappers.ConvertAppUserToMemberDto(pagedAppUser, following: true));
            }
            else
                memberDtos.Add(Mappers.ConvertAppUserToMemberDto(pagedAppUser));
        }

        return memberDtos;
    }
    
    [HttpPost("{targetMemberUserName}")]
    public async Task<ActionResult> Add(string targetMemberUserName, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserId(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return BadRequest("User id is invalid. Login again.");

        FollowStatus followStatus = await _followRepository.AddFollowAsync(userId.Value, targetMemberUserName, cancellationToken);

        return followStatus.IsSuccess // success
            ? Ok(new Response(Message: $"You are now following '{followStatus.KnownAs}'."))
            : followStatus.IsTargetMemberNotFound
            ? BadRequest($"'{followStatus.KnownAs}' is not found.")
            : followStatus.IsFollowingThemself
            ? BadRequest("Following yourself is great but is not stored!")
            : followStatus.IsAlreadyFollowed
            ? BadRequest($"{followStatus.KnownAs} is already followed.")
            : BadRequest("Follwoing has failed. Please try again later or contact the support");
    }

    [HttpDelete("{targetMemberUserName}")]
    public async Task<ActionResult> Delete(string targetMemberUserName, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserId(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return BadRequest("User id is invalid. Login again.");

        FollowStatus followStatus = await _followRepository.RemoveFollowAsync(userId.Value, targetMemberUserName, cancellationToken);

        return followStatus.IsSuccess
            ? Ok(new Response(Message: $"You've unfollowed '{followStatus.KnownAs}'."))
            : BadRequest("Operation failed. Is member already unfollowed?! Please try again later or contact the support");
    }
}
