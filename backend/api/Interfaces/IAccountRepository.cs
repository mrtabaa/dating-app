namespace api.Interfaces;

public interface IAccountRepository
{
    public Task<OperationResult<bool>> CreateAsync(RegisterDto registerDto, CancellationToken cancellationToken);
    public Task<OperationResult<LoggedInDto>> VerifyAsync(VerifyDto verifyDto, CancellationToken cancellationToken);
    public Task<OperationResult<bool>> ResendVerifyCodeAsync(ResendCodeRequest resendCodeRequest, CancellationToken cancellationToken);
    public Task<OperationResult<LoggedInDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken);
    public Task<OperationResult<LoggedInDto>> ReloadLoggedInUserAsync(string userIdHashed, string token, CancellationToken cancellationToken);
    public Task<OperationResult<bool>> RequestResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);
    public Task<OperationResult<bool>> ResetPasswordAsync(ResetPassword resetPassword, CancellationToken cancellationToken);
    public Task<OperationResult<DeleteResult>> DeleteUserAsync(string? userEmail, CancellationToken cancellationToken);
    public Task<OperationResult<UpdateResult>> UpdateLastActive(string loggedInUserIdHashed, CancellationToken cancellationToken);
}