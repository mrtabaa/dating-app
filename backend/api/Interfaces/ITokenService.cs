namespace api.Interfaces;
public interface ITokenService
{
    Task<string?> CreateToken(AppUser user, CancellationToken cancellationToken);
    Task<ObjectId?> GetDecryptedUserId(string securedId, CancellationToken cancellationToken);
}