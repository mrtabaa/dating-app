using api.Controllers.Helpers;
using api.Validations;

namespace api.Controllers;

[Authorize]
public class UserController(IUserRepository userRepository, ITokenService tokenService) : BaseApiController
{
    #region User Management

    [HttpPut]
    public async Task<ActionResult> UpdateUser(UserUpdateDto userUpdateDto, CancellationToken cancellationToken)
    {
        ObjectId? userId = await tokenService.GetActualUserIdAsync(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return Unauthorized("User id is invalid. Login again.");

        OperationResult result = await userRepository.UpdateUserAsync(
            userUpdateDto, userId.Value, cancellationToken
        );

        return result.IsSuccess
            ? Ok(new Response("Your information has been updated successfully."))
            : result.Error?.Code switch
            {
                UserErrorType.UpdateFailed => BadRequest(result.Error.Message),
                UserErrorType.InfoAlreadySaved => BadRequest(result.Error.Message),
                _ => BadRequest("Update failed. Contact support.")
            };
    }

    #endregion User Management

    #region Photo Management

    // only jpeg, jpg, png, webp. Between 50KB and 4MB(2000x2000)
    [HttpPost("add-photo")]
    public async Task<ActionResult<Photo>> AddPhoto(
        [AllowedFileExtensions] [FileSize(50_000, 2000 * 2000)]
        IFormFile file, CancellationToken cancellationToken
    )
    {
        ObjectId? userId = await tokenService.GetActualUserIdAsync(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return Unauthorized("User id is invalid. Login again.");

        /*                          ** Photo Upload Steps/Process **
            UserController => UserRepository: GetById() => PhotoService => PhotoModifySaveService
            PhotoService => UserRepository: MongoDb, return Photo => UserController
        */
        OperationResult<Photo> result = await userRepository.UploadPhotoAsync(
            file, userId.Value, cancellationToken
        );

        return result.IsSuccess
            ? result.Result
            : result.Error?.Code switch
            {
                UserErrorType.MaxPhotosLimitReached => BadRequest(result.Error.Message),
                _ => BadRequest("Photo upload failed. Try again later. If the issue persists contact the support.")
            };
    }

    [HttpPut("set-main-photo")]
    public async Task<ActionResult> SetMainPhoto(string photoUrlIn, CancellationToken cancellationToken)
    {
        ObjectId? userId = await tokenService.GetActualUserIdAsync(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return Unauthorized("User id is invalid. Login again.");

        OperationResult result = await userRepository.SetMainPhotoAsync(
            userId.Value, photoUrlIn, cancellationToken
        );

        return result.IsSuccess
            ? Ok(new Response("Set this photo as main succeeded."))
            : BadRequest(
                "Set as main photo failed. Try again in a few moments. If the issue persists contact the admin."
            );
    }

    [HttpDelete("delete-one-photo")]
    public async Task<ActionResult<PhotoDeleteResponse>> DeleteOnePhoto(
        string photoUrlIn, CancellationToken cancellationToken
    )
    {
        ObjectId? userId = await tokenService.GetActualUserIdAsync(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return Unauthorized("User id is invalid. Login again.");

        PhotoDeleteResponse photoDeleteResponse = await userRepository.DeletePhotoAsync(
            userId.Value, photoUrlIn, cancellationToken
        );

        if (photoDeleteResponse.IsDeletionFailed)
        {
            return BadRequest(
                "Photo deletion failed. Try again in a few moments. If the issue persists contact the support."
            );
        }

        photoDeleteResponse.SuccessMessage = "Photo got deleted successfully.";
        return photoDeleteResponse;
    }

    #endregion Photo Management
}