namespace api.Controllers;

[Authorize]
// [Produces("application/json")]
public class UserController : BaseApiController
{
    #region Variables and Constructor
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto?>>> GetUsers(CancellationToken cancellationToken)
    {
        return await _userRepository.GetUsers(cancellationToken);
    }
    #endregion Variables and Constructor

    #region User Management
    [HttpGet("id/{id}")]
    public async Task<ActionResult<MemberDto>> GetUserById(string id, CancellationToken cancellationToken)
    {
        MemberDto? user = await _userRepository.GetUserById(id, cancellationToken);

        return user is null ? BadRequest("No user found by this ID.") : user;
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<MemberDto>> GetUserByEmail(string email, CancellationToken cancellationToken)
    {
        MemberDto? user = await _userRepository.GetUserByEmail(email, cancellationToken);

        return user is null ? BadRequest("No user found by this Email.") : user;
    }

    [HttpPut()]
    public async Task<ActionResult<UpdateResult?>> UpdateUser(MemberUpdateDto memberUpdateDto, CancellationToken cancellationToken)
    {
        // MemberDto? user = await _userRepository.GetUserById(User.GetUserId(), cancellationToken);

        var result = await _userRepository.UpdateUser(memberUpdateDto, User.GetUserId(), cancellationToken);

        return result is null ? BadRequest("Update failed. See logger") : result;
    }

    [HttpDelete("delete-user/{userId}")]
    public async Task<ActionResult<DeleteResult>> DeleteUser(string userId, CancellationToken cancellationToken)
    {
        var result = await _userRepository.DeleteUser(userId, cancellationToken);
        return result is null ? BadRequest("Delete user failed!") : result;
    }
    #endregion User Management

    #region Photo Management
    [RequestSizeLimit(40_000_000)]
    [HttpPost("add-photos")]
    public async Task<ActionResult<UpdateResult>> AddPhotos([MaxFileSize(5_000_000), AllowedFileExtensions] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null) return BadRequest("No file is selected with this request.");

        var result = await _userRepository.UploadPhotos(file, User.GetUserId(), cancellationToken);

        return result is null ? BadRequest("Add photos failed. See logger") : result;
    }

    [HttpDelete("delete-one-photo")]
    public async Task<ActionResult<UpdateResult>> DeleteOnePhoto(string photoUrlIn, CancellationToken cancellationToken)
    {
        var result = await _userRepository.DeleteOnePhoto(User.GetUserId(), photoUrlIn, cancellationToken);

        return result is null ? BadRequest("Delete photo failed. See logger") : result;
    }

    [HttpPut("set-main-photo")]
    public async Task<ActionResult<UpdateResult?>> SetMainPhoto(string photoUrlIn, CancellationToken cancellationToken) => 
        await _userRepository.SetMainPhoto(User.GetUserId(), photoUrlIn, cancellationToken);
    #endregion Photo Management
}
