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
    public async Task<ActionResult<UserDto>> Register(UserRegisterDto userIn, CancellationToken cancellationToken)
    {
        if(userIn.Password != userIn.ConfirmPassword) return BadRequest("Password entries don't match!");

        UserDto? user = await _accountRepository.Create(userIn, cancellationToken);

        return user is null ? BadRequest("Email is already registered.") : user;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto userInput, CancellationToken cancellationToken)
    {
        UserDto? user = await _accountRepository.Login(userInput, cancellationToken);

        return user is null ? Unauthorized("Invalid username or password.") : user;
    }
}
