namespace api.Controllers;

[Authorize(Policy = "RequiredAdminRole")]
public class AdminController(IAdminRepository _adminRepository) : BaseApiController
{
    [HttpGet("users-with-roles")]
    public async Task<ActionResult<IEnumerable<MemberWithRoleDto>>> GetUsersWithRoles()
    {
        return Ok(await _adminRepository.GetUsersWithRolesAsync());
    }

    [HttpPut("edit-roles/{userName}")]
    public async Task<ActionResult<IList<string>>> EditRoles(string userName, [FromQuery] string newRoles)
    {
        IEnumerable<string>? result = await _adminRepository.EditMemberRole(userName, newRoles);

        return result is null ? BadRequest("Edit roles failed") : Ok(result);
    }

    // [Authorize(Policy = "ModeratePhotoRole")]
    // [HttpGet("photos-to-moderate")]
    // public async Task<ActionResult> GetPhotosForModeration()
    // {
    //     return Ok("Admins or Moderators can see this.");
    // }
}
