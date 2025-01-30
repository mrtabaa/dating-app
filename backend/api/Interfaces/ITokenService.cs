namespace api.Interfaces;

public interface ITokenService
{
    Task<ObjectId?> GetActualUserIdAsync(string? userIdHashed, CancellationToken cancellationToken);
    public Task<string> GenerateAccessTokenAsync(AppUser appUser, CancellationToken cancellationToken);
    public Task<string> GenerateRefreshTokenAsync(AppUser appUser);
}