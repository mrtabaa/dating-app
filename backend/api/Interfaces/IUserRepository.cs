namespace api.Interfaces;
public interface IUserRepository
{
    public Task<AppUser?> GetByIdAsync(ObjectId? userId, CancellationToken cancellationToken);
    public Task<AppUser?> GetByHashedIdAsync(string? userId, CancellationToken cancellationToken);
    public Task<AppUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken);
    public Task<ObjectId?> GetIdByUserNameAsync(string userName, CancellationToken cancellationToken);
    public Task<string?> GetKnownAsByUserNameAsync(string userName, CancellationToken cancellationToken);
    public Task<IdAndStringValue?> GetGenderByHashedIdAsync(string? userIdHashed, CancellationToken cancellationToken);

    public Task<UpdateResult?> UpdateUserAsync(UserUpdateDto userUpdateDto, string? userId, CancellationToken cancellationToken);
    public Task<PhotoUploadStatus> UploadPhotoAsync(IFormFile file, string? userId, CancellationToken cancellationToken);
    public Task<UpdateResult?> SetMainPhotoAsync(string? userId, string photoUrlIn, CancellationToken cancellationToken);
    public Task<UpdateResult?> DeletePhotoAsync(string? userId, string? urlIn, CancellationToken cancellationToken);
}