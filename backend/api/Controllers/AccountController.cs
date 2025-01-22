using api.DTOs.Account;
using api.DTOs.Helpers;
using Microsoft.Extensions.Primitives;

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
        OperationResult<LoggedInDto> result = await accountRepository.VerifyAsync(verifyDto, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Result)
            : result.Error?.Code switch
            {
                ErrorCode.IsEmailAlreadyConfirmed => Conflict(result.Error.Message),
                _ => BadRequest("Failed to verify your account. Check the code and try again.")
            };
    }

    [AllowAnonymous]
    [HttpPost("resend-verify-code")]
    public async Task<ActionResult<bool>> ResendVerifyCode(ResendCodeRequest resendCodeRequest, CancellationToken cancellationToken)
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
        OperationResult<LoggedInDto> result = await accountRepository.LoginAsync(userIn, cancellationToken);

        return result.IsSuccess
            ? result.Result
            : result.Error?.Code switch
            {
                ErrorCode.IsRecaptchaTokenInvalid => BadRequest(result.Error.Message),
                ErrorCode.IsWrongCreds => Unauthorized(result.Error.Message),
                ErrorCode.IsEmailNotConfirmed => BadRequest(result.Error.Message),
                _ => Unauthorized("Login has failed. Try again or contact the support.")
            };
    }

    [HttpGet]
    public async Task<ActionResult<LoggedInDto>> ReloadLoggedInUser(CancellationToken cancellationToken)
    {
        // Obtain token value
        string? token = null;

        if (HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authHeader))
            token = authHeader.ToString().Split(' ').Last();

        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token is expired or invalid. Login again.");

        string? userIdHashed = User.GetUserIdHashed();
        if (string.IsNullOrEmpty(userIdHashed))
            return Unauthorized("No user was found with this user Id.");

        OperationResult<LoggedInDto> result = await accountRepository.ReloadLoggedInUserAsync(userIdHashed, token, cancellationToken);

        return result.IsSuccess
            ? result.Result
            : Unauthorized("User is logged out or unauthorized. Login again.");
    }

    [AllowAnonymous]
    [HttpPost("request-reset-password")]
    public async Task<ActionResult<Response>> RequestResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
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
    public async Task<ActionResult<Response>> ResetPassword(ResetPassword resetPassword, CancellationToken cancellationToken)
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
        OperationResult<DeleteResult> result = await accountRepository.DeleteUserAsync(User.GetUserIdHashed(), cancellationToken);
        return result is { IsSuccess: true, Result.DeletedCount: > 0 } ? result.Result : BadRequest("Delete user failed!");
    }
}