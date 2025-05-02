using Da.Application.UseCaseInterfaces;

namespace Da.Application.UseCaseImplementations;

public class AccountService : IAccountService
{
    public async Task<OperationResult> CreateAsync(RegisterDto registerDto, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public async Task<OperationResult<LoginResult>> VerifyAsync(
        VerifyDto verifyDto, SessionMetadata sessionMetadata, CancellationToken cancellationToken
    ) => throw new NotImplementedException();

    public async Task<OperationResult> ResendVerifyCodeAsync(
        ResendCodeRequest resendCodeRequest, CancellationToken cancellationToken
    ) => throw new NotImplementedException();

    public async Task<OperationResult<LoginResult>> LoginAsync(
        LoginDto loginDto, SessionMetadata sessionMetadata, CancellationToken cancellationToken
    ) => throw new NotImplementedException();

    public async Task<OperationResult<TokenDto>> RefreshTokensAsync(
        RefreshTokenRequest refreshTokenRequest, CancellationToken cancellationToken
    ) => throw new NotImplementedException();

    public async Task<OperationResult<LoggedInDto>> ReloadLoggedInUserAsync(
        string userIdHashed, CancellationToken cancellationToken
    ) => throw new NotImplementedException();

    public async Task<OperationResult> RequestResetPasswordAsync(
        ResetPasswordRequest request, CancellationToken cancellationToken
    ) => throw new NotImplementedException();

    public async Task<OperationResult> ResetPasswordAsync(
        ResetPassword resetPassword, CancellationToken cancellationToken
    ) => throw new NotImplementedException();

    public async Task<OperationResult<bool>> DeleteUserAsync(string? userEmail, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public async Task<OperationResult> UpdateLastActive(
        string loggedInUserIdHashed, CancellationToken cancellationToken
    ) => throw new NotImplementedException();
}