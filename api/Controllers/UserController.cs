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

        return user == null ? BadRequest("No user found by this ID.") : user;
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<MemberDto>> GetUserByEmail(string email, CancellationToken cancellationToken)
    {
        MemberDto? user = await _userRepository.GetUserByEmail(email, cancellationToken);

        return user == null ? BadRequest("No user found by this Email.") : user;
    }

    [HttpPut()]
    public async Task<ActionResult<UpdateResult?>> UpdateUser(MemberUpdateDto memberUpdateDto, CancellationToken cancellationToken)
    {
        MemberDto? user = await _userRepository.GetUserById(User.GetUserId(), cancellationToken);

        return user == null
            ? BadRequest("Update failed.")
            : await _userRepository.UpdateUser(memberUpdateDto, User.GetUserId(), cancellationToken);
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<UpdateResult>> AddPhoto(IFormFile file, CancellationToken cancellationToken)
    {
        var result = await _userRepository.UploadPhoto(file, User.GetUserId(), cancellationToken);

        if (result == null)
            return BadRequest("Update failed. See logger");

        return result;
    }
}
