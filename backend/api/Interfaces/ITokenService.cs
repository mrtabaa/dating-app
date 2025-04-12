namespace api.Interfaces;

public interface ITokenService
{
    Task<ObjectId?> GetActualUserIdAsync(string? userIdHashed, CancellationToken cancellationToken);

    public Task<TokenDto> GenerateTokensAsync(
        RefreshTokenRequest refreshTokenRequest, AppUser appUser, CancellationToken cancellationToken
    );
}