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

    [HttpPut("update")]
    public async Task<ActionResult<UpdateResult?>> UpdateUser(MemberUpdateDto memberUpdateDto, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId != null)
        {
            MemberDto? user = await _userRepository.GetUserById(userId, cancellationToken);

            return user == null ? NotFound("User not found.") : await _userRepository.UpdateUser(memberUpdateDto, userId, cancellationToken);
        }

        return BadRequest("Update failed.");
    }
}
