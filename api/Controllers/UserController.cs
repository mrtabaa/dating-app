using Microsoft.AspNetCore.Authentication;

namespace api.Controllers;

[Authorize]
// [Produces("application/json")]
public class UserController(IUserRepository _userRepository) : BaseApiController
{
    #region User Management
    [HttpGet("id")]
    public async Task<ActionResult<LoggedInDto>> GetLoggedInUser(CancellationToken cancellationToken)
    {
        string? token = Response.HttpContext.GetTokenAsync("access_token").Result;

        LoggedInDto? loggedInDto = await _userRepository.GetLoggedInUserAsync(User.GetUserId(), token, cancellationToken);

        return loggedInDto is null ? BadRequest("Trouble finding the user!") : loggedInDto;
    }

    [HttpPut]
    public async Task<ActionResult<UpdateResult?>> UpdateUser(UserUpdateDto userUpdateDto, CancellationToken cancellationToken)
    {
        // UserDto? user = await _userRepository.GetUserById(User.GetUserId(), cancellationToken); TODO what is this?
        UpdateResult? result = await _userRepository.UpdateUserAsync(userUpdateDto, User.GetUserId(), cancellationToken);

        return result is null ? BadRequest("Update failed. See logger") : result;
    }

    // TODO remove userId
    [HttpDelete("delete-user/{userId}")]
    public async Task<ActionResult<DeleteResult>> DeleteUser(string userId, CancellationToken cancellationToken)
    {
        DeleteResult? result = await _userRepository.DeleteUserAsync(userId, cancellationToken);
        return result is null ? BadRequest("Delete user failed!") : result;
    }
    #endregion User Management

    #region Photo Management
    [RequestSizeLimit(4096 * 4096), AllowedFileExtensions] // only jpeg, jpg, png up to 4096 x 4096
    [HttpPost("add-photo")]
    public async Task<ActionResult<Photo>> AddPhoto(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null) return BadRequest("No file is selected with this request.");

        Photo? photo = await _userRepository.UploadPhotosAsync(file, User.GetUserId(), cancellationToken);

        return photo is null ? BadRequest("Add photo failed. See logger") : photo;
    }

    [HttpDelete("delete-one-photo")]
    public async Task<ActionResult<UpdateResult>> DeleteOnePhoto(string photoUrlIn, CancellationToken cancellationToken)
    {
        UpdateResult? result = await _userRepository.DeleteOnePhotoAsync(User.GetUserId(), photoUrlIn, cancellationToken);

        return result is null ? BadRequest("Delete photo failed. See logger") : result;
    }

    [HttpPut("set-main-photo")]
    public async Task<ActionResult<UpdateResult?>> SetMainPhoto(string photoUrlIn, CancellationToken cancellationToken) =>
        await _userRepository.SetMainPhotoAsync(User.GetUserId(), photoUrlIn, cancellationToken);
    #endregion Photo Management
}
