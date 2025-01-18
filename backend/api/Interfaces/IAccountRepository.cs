namespace api.Interfaces;

public interface IAccountRepository
{
    public Task<RegisteredDto> CreateAsync(RegisterDto registerDto, CancellationToken cancellationToken);
    public Task<LoggedInDto> VerifyAsync(VerifyDto verifyDto, CancellationToken cancellationToken);
    public Task<ResendCodeResult> ResendVerifyCodeAsync(ResendCodeRequest resendCodeRequest, CancellationToken cancellationToken);
    public Task<LoggedInDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken);
    public Task<LoggedInDto?> ReloadLoggedInUserAsync(string userIdHashed, string token, CancellationToken cancellationToken);
    public Task<bool> RequestResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);
    public Task<UpdateResult?> UpdateLastActive(string loggedInUserIdHashed, CancellationToken cancellationToken);
    public Task<DeleteResult?> DeleteUserAsync(string? userEmail, CancellationToken cancellationToken);
}