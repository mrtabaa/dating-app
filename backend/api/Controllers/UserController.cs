namespace api.Controllers;

[Authorize]
// [Produces("application/json")]
public class UserController(IUserRepository _userRepository, ITokenService _tokenService) : BaseApiController
{
    #region User Management
    [HttpPut]
    public async Task<ActionResult> UpdateUser(UserUpdateDto userUpdateDto, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserId(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return BadRequest("User id is invalid. Login again.");

        UpdateResult? updateResult = await _userRepository.UpdateUserAsync(userUpdateDto, userId.Value, cancellationToken);

        return updateResult is null || updateResult.MatchedCount == 0
            ? BadRequest("Update failed. Try again later or if the issue persists contact the support.")
            : !userUpdateDto.IsProfileCompleted || updateResult.MatchedCount == 1 && updateResult.ModifiedCount == 0
            ? BadRequest("This info is already saved.")
            : Ok(new Response(Message: "Your information has been updated successfully."));
    }
    #endregion User Management

    #region Photo Management
    // only jpeg, jpg, png. Between 100KB and 4MB(2000x2000)
    [HttpPost("add-photo")]
    public async Task<ActionResult<Photo>> AddPhoto([AllowedFileExtensions, FileSize(100_000, 2000 * 2000)] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null) return BadRequest("No file is selected with this request.");

        ObjectId? userId = await _tokenService.GetActualUserId(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return BadRequest("User id is invalid. Login again.");

        /*                          ** Photo Upload Steps/Process **
            UserController => UserRepository: GetById() => PhotoService => PhotoModifySaveService
            PhotoService => UserRepository: MongoDb, return Photo => UserController
        */
        PhotoUploadStatus photoUploadStatus = await _userRepository.UploadPhotoAsync(file, userId.Value, cancellationToken);

        if (photoUploadStatus.IsMaxPhotoReached)
            return BadRequest($"You've reach the limit of {photoUploadStatus.MaxPhotosLimit} photos. Delete some photos to add more.");

        return photoUploadStatus.Photo is null ? BadRequest("Photo upload failed. Try again later. If the issue persists contact the support.") : photoUploadStatus.Photo;
    }

    [HttpPut("set-main-photo")]
    public async Task<ActionResult> SetMainPhoto(string photoUrlIn, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserId(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return BadRequest("User id is invalid. Login again.");

        UpdateResult? updateResult = await _userRepository.SetMainPhotoAsync(userId.Value, photoUrlIn, cancellationToken);

        return updateResult is null || updateResult.ModifiedCount == 0
            ? BadRequest("Set as main photo failed. Try again in a few moments. If the issue persists contact the admin.")
            : Ok(new Response(Message: "Set this photo as main succeeded."));
    }

    [HttpDelete("delete-one-photo")]
    public async Task<ActionResult<PhotoDeleteResponse>> DeleteOnePhoto(string photoUrlIn, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserId(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return BadRequest("User id is invalid. Login again.");

        PhotoDeleteResponse photoDeleteResponse = await _userRepository.DeletePhotoAsync(userId.Value, photoUrlIn, cancellationToken);

        if (photoDeleteResponse.IsDeletionFailed)
            return BadRequest("Photo deletion failed. Try again in a few moments. If the issue persists contact the support.");

        photoDeleteResponse.SuccessMessage = "Photo got deleted successfully.";
        return photoDeleteResponse;
    }
    #endregion Photo Management
}
