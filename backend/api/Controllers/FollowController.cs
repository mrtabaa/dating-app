namespace api.Controllers;

[Authorize]
public class FollowController(IFollowRepository followRepository, ITokenService tokenService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetFollows(
        [FromQuery] FollowParams followParams, CancellationToken cancellationToken
    )
    {
        ObjectId? userId = await tokenService.GetActualUserIdAsync(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return Unauthorized("User id is invalid. Login again.");
        //

        OperationResult<PagedList<AppUser>> pagedAppUsersResult =
            await followRepository.GetFollowMembersAsync(userId.Value, followParams, cancellationToken);

        if (!pagedAppUsersResult.IsSuccess) return BadRequest("Getting members failed.");

        if (pagedAppUsersResult.Result.Count == 0) return NoContent();

        /*  1- Response only exists in the Controller. So we have to set PaginationHeader here before converting AppUser to UserDto.
                If we convert AppUser before here, we'll lose PagedList's pagination values, e.g., CurrentPage, PageSize, etc.
        */
        Response.AddPaginationHeader(
            new PaginationHeader(
                pagedAppUsersResult.Result.CurrentPage, pagedAppUsersResult.Result.PageSize,
                pagedAppUsersResult.Result.TotalItemsCount, pagedAppUsersResult.Result.TotalPages
            )
        );

        /*  2- PagedList<T> has to be AppUser first to retrieve data from DB and set pagination values.
                After that step, we can convert AppUser to MemberDto in here (NOT in the UserRepository) */
        List<MemberDto> memberDtos = [];

        foreach (AppUser pagedAppUser in pagedAppUsersResult.Result)
            if (await followRepository.CheckIsFollowing(userId.Value, pagedAppUser, cancellationToken))
                memberDtos.Add(Mappers.ConvertAppUserToMemberDto(pagedAppUser, true));
            else
                memberDtos.Add(Mappers.ConvertAppUserToMemberDto(pagedAppUser));

        return memberDtos;
    }

    [HttpPost("{targetMemberUserName}")]
    public async Task<ActionResult> Add(string targetMemberUserName, CancellationToken cancellationToken)
    {
        ObjectId? userId = await tokenService.GetActualUserIdAsync(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return Unauthorized("User id is invalid. Login again.");

        OperationResult result = await followRepository.AddFollowAsync(
            userId.Value, targetMemberUserName, cancellationToken
        );

        return result.IsSuccess
            ? Ok(new Response($"You are now following '{targetMemberUserName}'."))
            : result?.Error.Code switch
            {
                FollowErrorType.TargetMemberNotFound => NotFound($"'{targetMemberUserName}' is not found."),
                FollowErrorType.FollowingThemself => BadRequest("Following yourself is great but is not stored!"),
                FollowErrorType.AlreadyFollowed => BadRequest($"{targetMemberUserName} is already followed."),
                _ => BadRequest("Following failed. Please try again later or contact the support")
            };
    }

    [HttpDelete("{targetMemberUserName}")]
    public async Task<ActionResult> Delete(string targetMemberUserName, CancellationToken cancellationToken)
    {
        ObjectId? userId = await tokenService.GetActualUserIdAsync(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return Unauthorized("User id is invalid. Login again.");

        OperationResult result = await followRepository.RemoveFollowAsync(
            userId.Value, targetMemberUserName, cancellationToken
        );

        return result.IsSuccess
            ? Ok(new Response($"You've unfollowed '{targetMemberUserName}'."))
            : result.Error?.Code switch
            {
                FollowErrorType.TargetMemberNotFound => NotFound($"'{targetMemberUserName}' is not found."),
                _ => BadRequest(
                    "Unfollowing failed. Is member already unfollowed?! Please try again later or contact the support."
                )
            };
    }
}