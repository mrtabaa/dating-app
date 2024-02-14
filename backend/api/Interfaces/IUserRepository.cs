namespace api.Interfaces;
public interface IUserRepository
{
    public Task<AppUser?> GetByIdAsync(ObjectId userId, CancellationToken cancellationToken);

    public Task<AppUser?> GetByEmailAsync(string userEmail, CancellationToken cancellationToken);

    public Task<ObjectId?> GetIdByEmailAsync(string userEmail, CancellationToken cancellationToken);

    public Task<string?> GetKnownAsByEmailAsync(string userEmail, CancellationToken cancellationToken);

    public Task<UpdateResult?> UpdateUserAsync(UserUpdateDto userUpdateDto, string? Id, CancellationToken cancellationToken);

    public Task<UpdateResult?> UpdateLastActive(string loggedInUserId, CancellationToken cancellationToken);

    public Task<Photo?> UploadPhotoAsync(IFormFile file, string? userId, CancellationToken cancellationToken);

    public Task<UpdateResult?> SetMainPhotoAsync(string? userId, string photoUrlIn, CancellationToken cancellationToken);

    public Task<UpdateResult?> DeleteOnePhotoAsync(string? userId, string? urlIn, CancellationToken cancellationToken);
}