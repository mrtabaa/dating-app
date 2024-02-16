namespace api.Interfaces;
public interface ITokenService
{
    Task<string?> CreateToken(AppUser appUser, CancellationToken cancellationToken);
    Task<ObjectId?> GetActualUserId(string? userIdHashed, CancellationToken cancellationToken);
}