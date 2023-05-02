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
    public async Task<ActionResult<IEnumerable<MemberDto?>>> GetUsers()
    {
        return await _userRepository.GetUsers();
    }

    [HttpGet("id/{id}")]
    public async Task<ActionResult<MemberDto>> GetUserById(string id)
    {
        MemberDto? user = await _userRepository.GetUserById(id);

        return user == null ? BadRequest("No user found by this ID.") : user;
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<MemberDto>> GetUserByEmail(string email)
    {
        MemberDto? user = await _userRepository.GetUserByEmail(email);

        return user == null ? BadRequest("No user found by this Email.") : user;
    }
}
