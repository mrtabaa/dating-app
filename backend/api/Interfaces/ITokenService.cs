namespace api.Interfaces;
public interface ITokenService
{
    Task<string?> CreateToken(AppUser appUser, CancellationToken cancellationToken);
    Task<ObjectId?> GetActualUserIdAsync(string? userIdHashed, CancellationToken cancellationToken);
}