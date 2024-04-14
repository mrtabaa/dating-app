namespace api.Controllers;

[Produces("application/json")]
public class AccountController(IAccountRepository _accountRepository) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<LoggedInDto>> Register(RegisterDto userIn, CancellationToken cancellationToken)
    {
        if (userIn.Password != userIn.ConfirmPassword) return BadRequest("Password entries don't match!");

        LoggedInDto? loggedInDto = await _accountRepository.CreateAsync(userIn, cancellationToken);

        return loggedInDto.Token is null ? BadRequest("Registration has failed. Try again.") : loggedInDto;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoggedInDto>> Login(LoginDto userInput, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userInput.EmailUsername))
            return BadRequest("Email or Username is required");

        LoggedInDto? loggedInDto = await _accountRepository.LoginAsync(userInput, cancellationToken);

        if (loggedInDto.IsWrongCreds) return Unauthorized("Invalid username or password.");

        return loggedInDto.Token is null ? BadRequest("Login has failed. Try again.") : loggedInDto;
    }

    [Authorize]
    [HttpGet]
    public ActionResult AuthorizeLoggedInUser() =>
        Ok(new Response(Message: "token is still valid and user is authorized"));

    [Authorize]
    [HttpDelete("delete-user")]
    public async Task<ActionResult<DeleteResult>> DeleteUser(CancellationToken cancellationToken)
    {
        DeleteResult? result = await _accountRepository.DeleteUserAsync(User.GetUserIdHashed(), cancellationToken);
        return result is null ? BadRequest("Delete user failed!") : result;
    }
}
