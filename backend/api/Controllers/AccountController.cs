namespace api.Controllers;

[Produces("application/json")]
public class AccountController(IAccountRepository _accountRepository) : BaseApiController
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<LoggedInDto>> Register(UserRegisterDto userIn, CancellationToken cancellationToken)
    {
        if (userIn.Password != userIn.ConfirmPassword) return BadRequest("Password entries don't match!");

        LoggedInDto? loggedInDto = await _accountRepository.CreateAsync(userIn, cancellationToken);

        return loggedInDto is null ? BadRequest("Email is already registered.") : loggedInDto;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoggedInDto>> Login(LoginDto userInput, CancellationToken cancellationToken)
    {
        LoggedInDto? loggedInDto = await _accountRepository.LoginAsync(userInput, cancellationToken);

        return loggedInDto is null ? Unauthorized("Invalid username or password.") : loggedInDto;
    }

    [Authorize] //TODO Test it in client on refresh. Add to teach if works
    [HttpGet]
    public async Task<ActionResult<LoggedInDto>> GetLoggedInUser(CancellationToken cancellationToken)
    {
        string? token = Response.HttpContext.GetTokenAsync("access_token").Result;

        LoggedInDto? loggedInDto = await _accountRepository.GetLoggedInUserAsync(User.GetUserEmail(), token, cancellationToken);

        return loggedInDto is null ? BadRequest("Trouble finding the user!") : loggedInDto;
    }

    // its Authorized
    [HttpDelete("delete-user")]
    public async Task<ActionResult<DeleteResult>> DeleteUser(CancellationToken cancellationToken)
    {
        var temp = User.GetUserEmail();

        DeleteResult? result = await _accountRepository.DeleteUserAsync(User.GetUserEmail(), cancellationToken);
        return result is null ? BadRequest("Delete user failed!") : result;
    }
}
