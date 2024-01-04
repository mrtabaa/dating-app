namespace api.Controllers;

[AllowAnonymous] // never use this if you have [Authorize] on the mothods. [Authorize] gets ignored
[Produces("application/json")]
public class AccountController(IAccountRepository _accountRepository) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<LoggedInDto>> Register(UserRegisterDto userIn, CancellationToken cancellationToken)
    {
        if (userIn.Password != userIn.ConfirmPassword) return BadRequest("Password entries don't match!");

        LoggedInDto? loggedInDto = await _accountRepository.CreateAsync(userIn, cancellationToken);

        return loggedInDto is null ? BadRequest("Email is already registered.") : loggedInDto;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoggedInDto>> Login(LoginDto userInput, CancellationToken cancellationToken)
    {
        LoggedInDto? loggedInDto = await _accountRepository.LoginAsync(userInput, cancellationToken);

        return loggedInDto is null ? Unauthorized("Invalid username or password.") : loggedInDto;
    }
}
