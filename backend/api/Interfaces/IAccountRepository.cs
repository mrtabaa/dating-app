namespace api.Interfaces;

public interface IAccountRepository
{
    public Task<OperationResult> CreateAsync(RegisterDto registerDto, CancellationToken cancellationToken);

    public Task<OperationResult<LoginResult>> VerifyAsync(
        VerifyDto verifyDto, SessionMetadata sessionMetadata, CancellationToken cancellationToken
    );

    public Task<OperationResult> ResendVerifyCodeAsync(
        ResendCodeRequest resendCodeRequest, CancellationToken cancellationToken
    );

    public Task<OperationResult<LoginResult>> LoginAsync(
        LoginDto loginDto, SessionMetadata sessionMetadata, CancellationToken cancellationToken
    );

    public Task<OperationResult<TokenDto>> RefreshTokensAsync(
        RefreshTokenRequest refreshTokenRequest, CancellationToken cancellationToken
    );

    public Task<OperationResult<LoggedInDto>> ReloadLoggedInUserAsync(
        string userIdHashed, CancellationToken cancellationToken
    );

    public Task<OperationResult> RequestResetPasswordAsync(
        ResetPasswordRequest request, CancellationToken cancellationToken
    );

    public Task<OperationResult> ResetPasswordAsync(ResetPassword resetPassword, CancellationToken cancellationToken);
    public Task<OperationResult<DeleteResult>> DeleteUserAsync(string? userEmail, CancellationToken cancellationToken);

    public Task<OperationResult<UpdateResult>> UpdateLastActive(
        string loggedInUserIdHashed, CancellationToken cancellationToken
    );
}