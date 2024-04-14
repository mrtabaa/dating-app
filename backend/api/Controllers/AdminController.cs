namespace api.Controllers;

[Authorize(Policy = "RequiredAdminRole")]
public class AdminController(IAdminRepository _adminRepository) : BaseApiController
{
    [HttpGet("users-with-roles")]
    public async Task<ActionResult<IEnumerable<MemberWithRoleDto>>> GetUsersWithRoles()
    {
        return Ok(await _adminRepository.GetUsersWithRolesAsync());
    }

    [HttpPut("edit-roles")]
    public async Task<ActionResult<IList<string>>> EditRoles(MemberWithRoleDto memberWithRoleDto)
    {
        if (!memberWithRoleDto.Roles.Contains("member"))
            return BadRequest("Cannot remove member role!");

        IEnumerable<string>? result = await _adminRepository.EditMemberRole(memberWithRoleDto);

        return result is null ? BadRequest("Edit roles failed. Contact the admin if persists.") : Ok(result);
    }

    [HttpDelete("delete-user/{userName}")]
    public async Task<ActionResult> DeleteUser(string userName) =>
        await _adminRepository.DeleteUserAsync(userName) is null
        ? BadRequest($"No user exists with the username: {userName}.")
        : Ok($""" "{userName}" got deleted sucessfully.""");

    // [Authorize(Policy = "ModeratePhotoRole")]
    // [HttpGet("photos-to-moderate")]
    // public async Task<ActionResult> GetPhotosForModeration()
    // {
    //     return Ok("Admins or Moderators can see this.");
    // }
}
