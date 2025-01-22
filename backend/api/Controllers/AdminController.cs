namespace api.Controllers;

[Authorize(Policy = AppVariablesExtensions.RequiredAdminRole)]
public class AdminController(IAdminRepository adminRepository, UserManager<AppUser> userManager) : BaseApiController
{
    [HttpGet("users-with-roles")]
    public async Task<ActionResult<IEnumerable<UserWithRoleDto>>> GetUsersWithRoles(
        [FromQuery] AdminParams adminParams, CancellationToken cancellationToken
    )
    {
        PagedList<AppUser> pagedListAppUser =
            await adminRepository.GetUsersWithRolesAsync(adminParams, cancellationToken);

        if (pagedListAppUser.Count == 0) return NoContent();

        /*  1- Response only exists in the Controller. So we have to set PaginationHeader here before converting AppUser to UserDto.
                    If we convert AppUser before here, we'll lose PagedList's pagination values, e.g. CurrentPage, PageSize, etc.
            */
        Response.AddPaginationHeader(
            new PaginationHeader(
                pagedListAppUser.CurrentPage, pagedListAppUser.PageSize, pagedListAppUser.TotalItemsCount,
                pagedListAppUser.TotalPages
            )
        );

        /*  2- PagedList<T> has to be AppUser first to retrieve data from DB and set pagination values.
                    After that step, we can convert AppUser to MemberDto in here (NOT in the UserRepository) */

        List<UserWithRoleDto> usersWithRoles = [];

        foreach (AppUser appUser in pagedListAppUser)
        {
            IEnumerable<string> roles = await userManager.GetRolesAsync(appUser);

            usersWithRoles.Add(
                new UserWithRoleDto(
                    appUser.UserName!,
                    roles
                )
            );
        }

        return Ok(usersWithRoles);
    }

    [HttpPut("edit-roles")]
    public async Task<ActionResult<IList<string>>> EditRoles(UserWithRoleDto memberWithRoleDto)
    {
        if (!memberWithRoleDto.Roles.Contains("member"))
            return BadRequest("Cannot remove member role!");

        OperationResult<IEnumerable<string>> result = await adminRepository.EditMemberRole(memberWithRoleDto);

        return result.IsSuccess
            ? Ok(result)
            : BadRequest("Edit roles failed. Contact the admin if persists.");
    }

    [HttpPut("verify-by-username/{username}")]
    public async Task<ActionResult<MemberDto>> VerifyByUsername(string username, CancellationToken cancellationToken)
    {
        bool isSuccess = await adminRepository.VerifyByUsernameAsync(username, cancellationToken);

        return isSuccess
            ? Ok("Email is verified.")
            : BadRequest("Email verification failed.");
    }

    [HttpDelete("delete-member/{userName}")]
    public async Task<ActionResult> DeleteMember(string userName)
    {
        if (userName.ToLower() == "admin") return BadRequest("Admin cannot be deleted.");

        return await adminRepository.DeleteMemberAsync(userName) is null
            ? BadRequest($"""No user exists with the username "{userName}".""")
            : Ok(new Response($""" "{userName}" got deleted successfully."""));
    }

    [HttpPut("reset-connections-presence")]
    public async Task<ActionResult> ResetConnectionsPresence(CancellationToken cancellationToken)
    {
        UpdateResult updateResult = await adminRepository.ResetConnectionsPresenceAsync(cancellationToken);

        return updateResult.ModifiedCount > 0 ? Ok("ConnectedIds are reset.") : BadRequest("Failed.");
    }

    [HttpPut("reset-group-names")]
    public async Task<ActionResult> ResetGroupNames(CancellationToken cancellationToken)
    {
        UpdateResult updateResult = await adminRepository.ResetGroupNamesAsync(cancellationToken);

        return updateResult.ModifiedCount > 0 ? Ok("GroupNames are reset.") : BadRequest("Failed.");
    }

    // [Authorize(Policy = "ModeratePhotoRole")]
    // [HttpGet("photos-to-moderate")]
    // public async Task<ActionResult> GetPhotosForModeration()
    // {
    //     return Ok("Admins or Moderators can see this.");
    // }
}