namespace api.Controllers;

[Authorize]
// [Produces("application/json")]
public class UserController(IUserRepository _userRepository) : BaseApiController
{
    #region User Management
    [HttpPut]
    public async Task<ActionResult<UpdateResult?>> UpdateUser(UserUpdateDto userUpdateDto, CancellationToken cancellationToken)
    {
        UpdateResult? result = await _userRepository.UpdateUserAsync(userUpdateDto, User.GetUserEmail(), cancellationToken);

        return result is null ? BadRequest("Update failed. See logger") : result;
    }
    #endregion User Management

    #region Photo Management
    // only jpeg, jpg, png. Between 250KB(500x500) and 4MB(2000x2000)
    [HttpPost("add-photo")]
    public async Task<ActionResult<Photo>> AddPhoto([AllowedFileExtensions, FileSize(500 * 500, 2000 * 2000)] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null) return BadRequest("No file is selected with this request.");

        Photo? photo = await _userRepository.UploadPhotoAsync(file, User.GetUserEmail(), cancellationToken);

        return photo is null ? BadRequest("Add photo failed. See logger") : photo;
    }

    [HttpDelete("delete-one-photo")]
    public async Task<ActionResult<UpdateResult>> DeleteOnePhoto(string photoUrlIn, CancellationToken cancellationToken)
    {
        UpdateResult? result = await _userRepository.DeleteOnePhotoAsync(User.GetUserEmail(), photoUrlIn, cancellationToken);

        return result is null ? BadRequest("Delete photo failed. See logger") : result;
    }

    [HttpPut("set-main-photo")]
    public async Task<ActionResult<UpdateResult?>> SetMainPhoto(string photoUrlIn, CancellationToken cancellationToken) =>
        await _userRepository.SetMainPhotoAsync(User.GetUserEmail(), photoUrlIn, cancellationToken);
    #endregion Photo Management
}
