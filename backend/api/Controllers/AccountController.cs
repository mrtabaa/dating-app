namespace api.Controllers;

[Authorize]
[Produces("application/json")]
public class AccountController(
    IAntiforgery antiforgery,
    IAccountRepository accountRepository,
    ITokenCookieService tokenCookieService,
    ILogger<AccountController> logger
)
    : BaseApiController
{
    [AllowAnonymous]
    [HttpGet("get-csrf-token")]
    public IActionResult GetCsrfToken()
    {
        AntiforgeryTokenSet tokens = antiforgery.GetAndStoreTokens(HttpContext); // ✅ NOT GetAndStoreTokens()

        if (tokens.RequestToken is null)
        {
            logger.LogError($"{nameof(tokens.RequestToken)}, anti-forgery token is null");
            return StatusCode(
                StatusCodes.Status500InternalServerError, $"{nameof(tokens.RequestToken)}, anti-forgery token is null"
            );
        }

        return Ok(new { requestToken = tokens.RequestToken }); // Return the RequestToken in the JSON
    }

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
                ErrorCode.NetIdentityFailed => BadRequest(result.Error.Message),
                _ => BadRequest("Registration has failed. Try again or contact the support.")
            };
    }

    [AllowAnonymous]
    [HttpPost("verify")]
    public async Task<ActionResult<LoggedInDto>> Verify(VerifyDto verifyDto, CancellationToken cancellationToken)
    {
        OperationResult<LoginResult> result = await accountRepository.VerifyAsync(
            verifyDto, ExtractSessionMetadata(), cancellationToken
        );

        if (!result.IsSuccess)
        {
            return result.Error?.Code switch
            {
                ErrorCode.IsEmailAlreadyConfirmed => Conflict(result.Error.Message),
                _ => BadRequest("Failed to verify your account. Check the code and try again.")
            };
        }

        AddTokensToResponseCookies(result.Result.TokenDto);
        return result.Result.LoggedInDto;
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
        OperationResult<LoginResult> result = await accountRepository.LoginAsync(
            userIn, ExtractSessionMetadata(), cancellationToken
        );

        if (!result.IsSuccess)
        {
            return result.Error?.Code switch
            {
                ErrorCode.IsRecaptchaTokenInvalid => BadRequest(result.Error.Message),
                ErrorCode.IsWrongCreds => Unauthorized(result.Error.Message),
                ErrorCode.IsEmailNotConfirmed => Accepted(result.Result.LoggedInDto),
                _ => Unauthorized("Login has failed. Try again or contact the support.")
            };
        }

        AddTokensToResponseCookies(result.Result.TokenDto);
        return result.Result.LoggedInDto;
    }

    [AllowAnonymous]
    [HttpGet("refresh-tokens")]
    public async Task<IActionResult> RefreshTokens(CancellationToken cancellationToken)
    {
        bool isSuccess = Request.Cookies.TryGetValue("auth.refresh-token", out string? protectedRefreshToken);
        if (!isSuccess || string.IsNullOrEmpty(protectedRefreshToken))
            return Unauthorized("Your session has expired. Login again.");

        // Decrypt RefreshTokenDto
        RefreshTokenRequest refreshTokenRequest = tokenCookieService.DecryptRefreshTokenRequest(protectedRefreshToken);

        refreshTokenRequest.SessionMetadata = ExtractSessionMetadata();

        OperationResult<TokenDto>
            result = await accountRepository.RefreshTokensAsync(refreshTokenRequest, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error?.Code switch
            {
                ErrorCode.IsSessionExpired => Unauthorized(result.Error.Message),
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
        #region Delete Tokens

        Response.Cookies.Delete(
            "auth.access-token", new CookieOptions
            {
                Path = "/"
            }
        );
        Response.Cookies.Delete(
            "auth.refresh-token", new CookieOptions
            {
                Path = "/api/account/refresh-tokens"
            }
        );

        #endregion

        #region Add Tokens

        // Access Token
        Response.Cookies.Append(
            "auth.access-token", tokenDto.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                // TODO: Create an obsidian document for token management
                // Use 'SameSiteMode.lax' if using OAuth, payments sites, etc.
                // Also implement CSRF Tokens to prevent CSRF attacks
                SameSite = SameSiteMode.None,
                // Domain = ".hallboard.com", // Ensures the cookie is valid across subdomains (e.g., www.hallboard.com, api.hallboard.com)
                Expires = DateTimeExtensions.GetTokenExpirationDate(tokenDto.AccessToken), // e.g. 15 min,
                Path = "/"
            }
        );

        // Refresh Token
        string encryptedCookie = tokenCookieService.EncryptRefreshTokenResponse(tokenDto.RefreshTokenResponse);

        Response.Cookies.Append(
            "auth.refresh-token", encryptedCookie, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                // Domain = ".hallboard.com",
                Expires = tokenDto.RefreshTokenResponse.ExpiresAt.UtcDateTime,
                Path = "/api/account/refresh-tokens"
            }
        );

        #endregion
    }

    private SessionMetadata ExtractSessionMetadata()
    {
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
        Parser? parser = Parser.GetDefault();
        ClientInfo? client = parser.Parse(userAgent);

        string deviceType = client.Device.IsSpider ? "Bot" :
            string.IsNullOrWhiteSpace(client.Device.Family) ? "Unknown" : client.Device.Family;
        var os = $"{client.OS.Family} {client.OS.Major}";
        var browser = $"{client.Browser.Family} {client.Browser.Major}";

        var deviceName = $"{os} - {browser}";

        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        // Optionally get location from headers (e.g., behind Cloudflare)
        string location = HttpContext.Request.Headers["CF-IPCountry"].FirstOrDefault() ?? "Unknown";

        return new SessionMetadata(
            deviceType,
            deviceName,
            string.IsNullOrWhiteSpace(userAgent) ? "Unknown" : userAgent,
            ipAddress,
            location
        );
    }
}