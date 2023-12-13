namespace api.Controllers;

[Authorize]
// [Produces("application/json")]
public class UserController(IUserRepository _userRepository) : BaseApiController
{
    #region Variables and Constructor

    #endregion Variables and Constructor

    #region User Management
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto?>>> GetUsers([FromQuery] UserParams userParams, CancellationToken cancellationToken)
    {
        List<UserDto?> userDtos = new();

        UserDto? currentUser = await _userRepository.GetUserByIdAsync(User.GetUserId(), cancellationToken);

        if (currentUser is not null && string.IsNullOrEmpty(userParams.Gender))
        {
            userParams.CurrentUserId = currentUser.Id;
            userParams.Gender = currentUser.Gender;
        }

        PagedList<AppUser> pagedAppUsers = await _userRepository.GetUsersAsync(userParams, cancellationToken);

        /*  1- Response only exists in Contoller. So we have to set PaginationHeader here before converting AppUser to UserDto.
                If we convert AppUser before here, we'll lose PagedList's pagination values, e.g. CurrentPage, PageSize, etc.
        */
        Response.AddPaginationHeader(new PaginationHeader(pagedAppUsers.CurrentPage, pagedAppUsers.PageSize, pagedAppUsers.TotalCount, pagedAppUsers.TotalPages));

        /*  2- PagedList<T> has to be AppUser first to retrieve data from DB and set pagination values. 
                After that step we can convert AppUser to UserDto in here (NOT in the UserRepository) */
        foreach (AppUser pagedAppUser in pagedAppUsers)
        {
            userDtos.Add(Mappers.GenerateUserDto(pagedAppUser));
        }

        return userDtos;
    }

    [HttpGet("id/{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(string id, CancellationToken cancellationToken)
    {
        UserDto? user = await _userRepository.GetUserByIdAsync(id, cancellationToken);

        return user is null ? BadRequest("No user found by this ID.") : user;
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<UserDto>> GetUserByEmail(string email, CancellationToken cancellationToken)
    {
        UserDto? user = await _userRepository.GetUserByEmailAsync(email, cancellationToken);

        return user is null ? BadRequest("No user found by this Email.") : user;
    }

    [HttpPut()]
    public async Task<ActionResult<UpdateResult?>> UpdateUser(UserUpdateDto userUpdateDto, CancellationToken cancellationToken)
    {
        // UserDto? user = await _userRepository.GetUserById(User.GetUserId(), cancellationToken);

        var result = await _userRepository.UpdateUserAsync(userUpdateDto, User.GetUserId(), cancellationToken);

        return result is null ? BadRequest("Update failed. See logger") : result;
    }

    [HttpDelete("delete-user/{userId}")]
    public async Task<ActionResult<DeleteResult>> DeleteUser(string userId, CancellationToken cancellationToken)
    {
        var result = await _userRepository.DeleteUserAsync(userId, cancellationToken);
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
        var result = await _userRepository.DeleteOnePhotoAsync(User.GetUserId(), photoUrlIn, cancellationToken);

        return result is null ? BadRequest("Delete photo failed. See logger") : result;
    }

    [HttpPut("set-main-photo")]
    public async Task<ActionResult<UpdateResult?>> SetMainPhoto(string photoUrlIn, CancellationToken cancellationToken) =>
        await _userRepository.SetMainPhotoAsync(User.GetUserId(), photoUrlIn, cancellationToken);
    #endregion Photo Management
}
