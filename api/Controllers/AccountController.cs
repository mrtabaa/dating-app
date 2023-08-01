namespace api.Controllers;

[AllowAnonymous] // never use this if you have [Authorize] on the mothods. [Authorize] gets ignored
[Produces("application/json")]
public class AccountController : BaseApiController
{
    private readonly IAccountRepository _accountRepository;

    public AccountController(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(UserRegisterDto userIn)
    {
        if(userIn.Password != userIn.ConfirmPassword) return BadRequest("Password entries don't match!");

        UserDto? user = await _accountRepository.Create(userIn);

        return user is null ? BadRequest("Email is already registered.") : user;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto userInput)
    {
        UserDto? user = await _accountRepository.Login(userInput);

        return user is null ? Unauthorized("Invalid username or password.") : user;
    }
}
