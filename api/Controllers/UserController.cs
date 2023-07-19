namespace api.Controllers;

[Authorize]
// [Produces("application/json")]
public class UserController : BaseApiController
{
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

    [RequestSizeLimit(40_000_000)]
    [HttpPost("add-photos")]
    public async Task<ActionResult<UpdateResult>> AddPhotos([MaxFileSize(5_000_000), AllowedFileExtensions] IFormFileCollection files, CancellationToken cancellationToken)
    {
        if (!files.Any()) return BadRequest("Please select a file.");

        var result = await _userRepository.UploadPhotos(files, User.GetUserId(), cancellationToken);

        return result is null ? BadRequest("Add photos failed. See logger") : result;
    }

    [HttpDelete("delete-one-photo")]
    public async Task<ActionResult<UpdateResult>> DeleteOnePhoto([FromBody] string photoUrl, CancellationToken cancellationToken)
    {
        var result = await _userRepository.DeleteOnePhoto(User.GetUserId(), photoUrl, cancellationToken);

        return result is null ? BadRequest("Delete photo failed. See logger") : result;
    }

    [HttpPut("set-main-photo/{photoUrlIn}")]
    public async Task<ActionResult<UpdateResult?>> SetMainPhoto(string photoUrlIn, CancellationToken cancellationToken) => 
        await _userRepository.SetMainPhoto(User.GetUserId(), photoUrlIn, cancellationToken);
}
