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
    public async Task<ActionResult<LoggedInDto>> AuthorizeLoggedInUser(CancellationToken cancellationToken)
    {
        string? userIdHashed = User.GetUserIdHashed();
        if (string.IsNullOrEmpty(userIdHashed))
            return BadRequest("No user was found with this user Id.");

        // obtain token value
        string? token = null;

        if (HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            token = authHeader.ToString().Split(' ').Last();

        if (string.IsNullOrEmpty(token))
            return BadRequest("Token is expired or invalid. Login again.");

        LoggedInDto? loggedInDto = await _accountRepository.ReloadLoggedInUser(userIdHashed, token, cancellationToken);

        return loggedInDto is null ? Unauthorized("User is logged out or unauthorized. Login again.") : loggedInDto;
    }

    [Authorize]
    [HttpDelete("delete-user")]
    public async Task<ActionResult<DeleteResult>> DeleteUser(CancellationToken cancellationToken)
    {
        DeleteResult? result = await _accountRepository.DeleteUserAsync(User.GetUserIdHashed(), cancellationToken);
        return result is null ? BadRequest("Delete user failed!") : result;
    }
}
