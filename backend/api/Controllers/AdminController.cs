namespace api.Controllers;

public class AdminController : BaseApiController
{
    [Authorize(Policy = "RequiredAdminRole")]
    [HttpGet("users-with-roles")]
    public ActionResult GetUsersWithRoles()
    {
        return Ok("Only Admins get users with roles.");
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public ActionResult GetPhotosForModeration()
    {
        return Ok("Admins or Moderators can see this.");
    }
}
