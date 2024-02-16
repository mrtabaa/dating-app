namespace api.Controllers;

[Authorize]
// [Produces("application/json")]
public class UserController(IUserRepository _userRepository) : BaseApiController
{
    #region User Management
    [HttpPut]
    public async Task<ActionResult<string>> UpdateUser(UserUpdateDto userUpdateDto, CancellationToken cancellationToken)
    {
        UpdateResult? updateResult = await _userRepository.UpdateUserAsync(userUpdateDto, User.GetUserIdHashed(), cancellationToken);

        return updateResult is null || updateResult.ModifiedCount == 0
            ? BadRequest("Update failed. Try again later.")
            : Ok(new { message = "User has been updated successfully." });

    }
    #endregion User Management

    #region Photo Management
    // only jpeg, jpg, png. Between 250KB(500x500) and 4MB(2000x2000)
    [HttpPost("add-photo")]
    public async Task<ActionResult<Photo>> AddPhoto([AllowedFileExtensions, FileSize(500 * 500, 2000 * 2000)] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null) return BadRequest("No file is selected with this request.");

        /*                          ** Photo Upload Steps/Process **
            UserController => UserRepository: GetById() => PhotoService => PhotoModifySaveService
            PhotoService => UserRepository: MongoDb, return Photo => UserController
        */
        Photo? photo = await _userRepository.UploadPhotoAsync(file, User.GetUserIdHashed(), cancellationToken);

        return photo is null ? BadRequest("Add photo failed. See logger") : photo;
    }

    [HttpPut("set-main-photo")]
    public async Task<ActionResult<string>> SetMainPhoto(string photoUrlIn, CancellationToken cancellationToken)
    {
        UpdateResult? updateResult = await _userRepository.SetMainPhotoAsync(User.GetUserIdHashed(), photoUrlIn, cancellationToken);

        return updateResult is null || updateResult.ModifiedCount == 0
            ? BadRequest("Set as main photo failed. Try again in a few moments. If the issue persists contact the admin.")
            : Ok(new { message = "Set this photo as main succeeded." });
    }

    [HttpDelete("delete-one-photo")]
    public async Task<ActionResult<string>> DeleteOnePhoto(string photoUrlIn, CancellationToken cancellationToken)
    {
        UpdateResult? updateResult = await _userRepository.DeletePhotoAsync(User.GetUserIdHashed(), photoUrlIn, cancellationToken);

        return updateResult is null || updateResult.ModifiedCount == 0
            ? BadRequest("Photo deletion failed. Try again in a few moments. If the issue persists contact the admin.")
            : Ok(new { message = "Photo deleted successfully." });
    }
    #endregion Photo Management
}
