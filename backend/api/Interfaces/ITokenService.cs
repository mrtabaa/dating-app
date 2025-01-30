namespace api.Interfaces;

public interface ITokenService
{
    Task<ObjectId?> GetActualUserIdAsync(string? userIdHashed, CancellationToken cancellationToken);
    public Task<TokenDto> GenerateTokensAsync(AppUser appUser, CancellationToken cancellationToken);
}