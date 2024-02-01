namespace api.Interfaces;
public interface IUserRepository
{
    public Task<AppUser?> GetByIdAsync(string? userId, CancellationToken cancellationToken);

    public Task<AppUser?> GetByEmailAsync(string? userEmail, CancellationToken cancellationToken);

    public Task<UpdateResult?> UpdateUserAsync(UserUpdateDto userUpdateDto, string? Id, CancellationToken cancellationToken);

    public Task<UpdateResult?> UpdateLastActive(string loggedInUserId, CancellationToken cancellationToken);

    public Task<DeleteResult?> DeleteUserAsync(string? userId, CancellationToken cancellationToken);

    public Task<Photo?> UploadPhotoAsync(IFormFile file, string? userId, CancellationToken cancellationToken);

    public Task<UpdateResult?> DeleteOnePhotoAsync(string? userId, string? urlIn, CancellationToken cancellationToken);

    public Task<UpdateResult?> SetMainPhotoAsync(string? userId, string photoUrlIn, CancellationToken cancellationToken);

}