using api.Controllers.Helpers;

namespace api.Controllers;

[Authorize]
[Produces("application/json")]
public class AccountController(IAccountRepository accountRepository) : BaseApiController
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<bool>> Register(RegisterDto userIn, CancellationToken cancellationToken)
    {
        if (userIn.Password != userIn.ConfirmPassword) return BadRequest("Password entries don't match!");

        OperationResult result = await accountRepository.CreateAsync(userIn, cancellationToken);

        return result.IsSuccess
            ? true
            : result.Error?.Code switch // Make sure to use null-conditional operator ? to avoid exceptions 
            {
                ErrorCode.IsRecaptchaTokenInvalid => BadRequest(result.Error.Message),
                ErrorCode.IsEmailAlreadyConfirmed => Conflict(result.Error.Message),
                ErrorCode.NetIdentity => BadRequest(result.Error.Message),
                _ => BadRequest("Registration has failed. Try again or contact the support.")
            };
    }

    [AllowAnonymous]
    [HttpPost("verify")]
    public async Task<ActionResult<LoggedInDto>> Verify(VerifyDto verifyDto, CancellationToken cancellationToken)
    {
        OperationResult<LoginResult> result = await accountRepository.VerifyAsync(verifyDto, cancellationToken);

        if (result.IsSuccess)
            AddTokensToResponseCookies(result.Result.TokenDto);

        return result.IsSuccess
            ? result.Result.LoggedInDto
            : result.Error?.Code switch
            {
                ErrorCode.IsEmailAlreadyConfirmed => Conflict(result.Error.Message),
                _ => BadRequest("Failed to verify your account. Check the code and try again.")
            };
    }

    [AllowAnonymous]
    [HttpPost("resend-verify-code")]
    public async Task<ActionResult<bool>> ResendVerifyCode(
        ResendCodeRequest resendCodeRequest, CancellationToken cancellationToken
    )
    {
        OperationResult result = await accountRepository.ResendVerifyCodeAsync(resendCodeRequest, cancellationToken);

        return result.IsSuccess
            ? true
            : result.Error?.Code switch
            {
                ErrorCode.IsRecaptchaTokenInvalid => BadRequest(result.Error.Message),
                ErrorCode.IsEmailAlreadyConfirmed => Conflict(result.Error.Message),
                _ => BadRequest("Failed to resend code. Try again or contact the support.")
            };
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoggedInDto>> Login(LoginDto userIn, CancellationToken cancellationToken)
    {
        OperationResult<LoginResult> result = await accountRepository.LoginAsync(userIn, cancellationToken);

        if (result.IsSuccess)
            AddTokensToResponseCookies(result.Result.TokenDto);

        return result.IsSuccess
            ? result.Result.LoggedInDto
            : result.Error?.Code switch
            {
                ErrorCode.IsRecaptchaTokenInvalid => BadRequest(result.Error.Message),
                ErrorCode.IsWrongCreds => Unauthorized(result.Error.Message),
                ErrorCode.IsEmailNotConfirmed => Accepted(result.Result.LoggedInDto),
                _ => Unauthorized("Login has failed. Try again or contact the support.")
            };
    }

    [HttpPost("refresh-tokens")]
    public async Task<ActionResult> RefreshTokens(CancellationToken cancellationToken)
    {
        string? identifierHash = User.GetUserIdHashed();
        if (string.IsNullOrEmpty(identifierHash))
            return Unauthorized("Your login session has expired. Please login again.");

        OperationResult<TokenDto>
            result = await accountRepository.RefreshTokensAsync(identifierHash, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error?.Code switch
            {
                ErrorCode.IsRefreshTokenExpired => Unauthorized(result.Error.Message),
                _ => BadRequest("Failed to refresh token. Try again or contact the support.")
            };
        }

        AddTokensToResponseCookies(result.Result);
        return Ok("Tokens refreshed successfully.");
    }

    [HttpGet]
    public async Task<ActionResult<LoggedInDto>> ReloadLoggedInUser(CancellationToken cancellationToken)
    {
        string? userIdHashed = User.GetUserIdHashed();
        if (string.IsNullOrEmpty(userIdHashed))
            return Unauthorized("No user was found with this user Id.");

        OperationResult<LoggedInDto> result = await accountRepository.ReloadLoggedInUserAsync(
            userIdHashed, cancellationToken
        );

        return result.IsSuccess
            ? result.Result
            : Unauthorized("User is logged out or unauthorized. Login again.");
    }

    [AllowAnonymous]
    [HttpPost("request-reset-password")]
    public async Task<ActionResult<Response>> RequestResetPassword(
        ResetPasswordRequest request, CancellationToken cancellationToken
    )
    {
        OperationResult result = await accountRepository.RequestResetPasswordAsync(request, cancellationToken);

        return result.Error?.Code switch
        {
            ErrorCode.IsRecaptchaTokenInvalid => BadRequest("Recaptcha token is invalid. 'Slide me!' again."),
            _ => new Response("If the email is registered and verified, a reset link will be sent to your email.")
        };
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<ActionResult<Response>> ResetPassword(
        ResetPassword resetPassword, CancellationToken cancellationToken
    )
    {
        if (resetPassword.Password != resetPassword.ConfirmPassword) return BadRequest("Password entries don't match!");

        OperationResult result = await accountRepository.ResetPasswordAsync(resetPassword, cancellationToken);

        return result.IsSuccess
            ? new Response("Your new password is saved successfully.")
            : BadRequest("""Reset password token is expired. Try "Forgot your password" again.""");
    }

    [HttpDelete("delete-account")]
    public async Task<ActionResult<DeleteResult>> DeleteUser(CancellationToken cancellationToken)
    {
        OperationResult<DeleteResult> result = await accountRepository.DeleteUserAsync(
            User.GetUserIdHashed(), cancellationToken
        );
        return result is { IsSuccess: true, Result.DeletedCount: > 0 }
            ? result.Result
            : BadRequest("Delete user failed!");
    }

    private void AddTokensToResponseCookies(TokenDto tokenDto)
    {
        Response.Cookies.Append(
            "access-token", tokenDto.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                // TODO: Create an obsidian document for token management
                // Use 'SameSiteMode.lax' if using OAuth, payments sites, etc.
                // Also implement CSRF Tokens to prevent CSRF attacks
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeExtensions.GetTokenExpirationDate(tokenDto.AccessToken), // e.g. 15 min,
                Path = "/"
            }
        );

        Response.Cookies.Append(
            "refresh-token", tokenDto.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeExtensions.GetTokenExpirationDate(tokenDto.RefreshToken), // e.g. 7 days
                Path = "/api/account/refresh-tokens"
            }
        );
    }
}