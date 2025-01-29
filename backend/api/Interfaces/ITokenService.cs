namespace api.Interfaces;

public interface ITokenService
{
    Task<string?> CreateToken(AppUser appUser, CancellationToken cancellationToken);
    Task<ObjectId?> GetActualUserIdAsync(string? userIdHashed, CancellationToken cancellationToken);
    public Task<string> GenerateAccessTokenAsync(AppUser appUser, CancellationToken cancellationToken);
    public Task<string> GenerateRefreshTokenAsync(AppUser appUser);
}