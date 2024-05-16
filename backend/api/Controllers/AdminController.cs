namespace api.Controllers;

[Authorize(Policy = "RequiredAdminRole")]
public class AdminController(IAdminRepository _adminRepository, UserManager<AppUser> _userManager) : BaseApiController
{
    [HttpGet("users-with-roles")]
    public async Task<ActionResult<IEnumerable<UserWithRoleDto>>> GetUsersWithRoles([FromQuery] PaginationParams paginationParams, CancellationToken cancellationToken)
    {
        PagedList<AppUser> pagedAppUsers = await _adminRepository.GetUsersWithRolesAsync(paginationParams, cancellationToken);

        if (pagedAppUsers is null) return BadRequest("Getting members faild");

        if (pagedAppUsers.Count == 0) return NoContent();

        /*  1- Response only exists in Contoller. So we have to set PaginationHeader here before converting AppUser to UserDto.
                If we convert AppUser before here, we'll lose PagedList's pagination values, e.g. CurrentPage, PageSize, etc.
        */
        Response.AddPaginationHeader(new PaginationHeader(pagedAppUsers.CurrentPage, pagedAppUsers.PageSize, pagedAppUsers.TotalItemsCount, pagedAppUsers.TotalPages));

        /*  2- PagedList<T> has to be AppUser first to retrieve data from DB and set pagination values. 
                After that step we can convert AppUser to MemberDto in here (NOT in the UserRepository) */

        List<UserWithRoleDto> usersWithRoles = [];
        
        foreach (AppUser appUser in pagedAppUsers)
        {
            IEnumerable<string> roles = await _userManager.GetRolesAsync(appUser);

            usersWithRoles.Add(
                new UserWithRoleDto(
                    UserName: appUser.UserName!,
                    Roles: roles
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

        IEnumerable<string>? result = await _adminRepository.EditMemberRole(memberWithRoleDto);

        return result is null ? BadRequest("Edit roles failed. Contact the admin if persists.") : Ok(result);
    }

    [HttpDelete("delete-member/{userName}")]
    public async Task<ActionResult> DeleteMember(string userName)
    {
        if (userName.ToLower() == "admin") return BadRequest("Admin cannot be deleted.");

        return await _adminRepository.DeleteMemberAsync(userName) is null
                ? BadRequest($"""No user exists with the username "{userName}".""")
                : Ok(new Response(Message: $""" "{userName}" got deleted sucessfully."""));
    }

    // [Authorize(Policy = "ModeratePhotoRole")]
    // [HttpGet("photos-to-moderate")]
    // public async Task<ActionResult> GetPhotosForModeration()
    // {
    //     return Ok("Admins or Moderators can see this.");
    // }
}
