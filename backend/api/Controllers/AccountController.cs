namespace api.Controllers;

[Authorize]
[Produces("application/json")]
public class AccountController(IAccountRepository _accountRepository) : BaseApiController
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<LoggedInDto>> Register(RegisterDto userIn, CancellationToken cancellationToken)
    {
        if (userIn.Password != userIn.ConfirmPassword) return BadRequest("Password entries don't match!");

        LoggedInDto? loggedInDto = await _accountRepository.CreateAsync(userIn, cancellationToken);

        return !string.IsNullOrEmpty(loggedInDto.Token) // success
            ? Ok(loggedInDto)
            : loggedInDto.IsRecaptchaTokenInvalid
            ? BadRequest("Recaptcha token is invalid. 'Slide me!' again.")
            : (loggedInDto.Errors.Count != 0)
            ? BadRequest(loggedInDto.Errors)
            : BadRequest("Registration has failed. Try again or contact the support.");
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoggedInDto>> Login(LoginDto userIn, CancellationToken cancellationToken)
    {
        LoggedInDto? loggedInDto = await _accountRepository.LoginAsync(userIn, cancellationToken);

        return !string.IsNullOrEmpty(loggedInDto.Token) // success
            ? Ok(loggedInDto)
            : loggedInDto.IsRecaptchaTokenInvalid
            ? BadRequest("Recaptcha token is invalid. 'Slide me!' again.")
            : loggedInDto.IsWrongCreds
            ? Unauthorized("Wrong username or password.")
            : (loggedInDto.Errors.Count != 0)
            ? BadRequest(loggedInDto.Errors)
            : Unauthorized("Login has failed. Try again or contact the support.");
    }

    [HttpGet]
    public async Task<ActionResult<LoggedInDto>> ReloadLoggedInUser(CancellationToken cancellationToken)
    {
        // obtain token value
        string? token = null;

        if (HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            token = authHeader.ToString().Split(' ').Last();

        if (string.IsNullOrEmpty(token))
            return BadRequest("Token is expired or invalid. Login again.");

        string? userIdHashed = User.GetUserIdHashed();
        if (string.IsNullOrEmpty(userIdHashed))
            return BadRequest("No user was found with this user Id.");

        LoggedInDto? loggedInDto = await _accountRepository.ReloadLoggedInUserAsync(userIdHashed, token, cancellationToken);

        return loggedInDto is null ? Unauthorized("User is logged out or unauthorized. Login again.") : loggedInDto;
    }

    [HttpDelete("delete-user")]
    public async Task<ActionResult<DeleteResult>> DeleteUser(CancellationToken cancellationToken)
    {
        DeleteResult? result = await _accountRepository.DeleteUserAsync(User.GetUserIdHashed(), cancellationToken);
        return result is null ? BadRequest("Delete user failed!") : result;
    }
}
