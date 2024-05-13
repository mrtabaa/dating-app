namespace api.Interfaces;
public interface IUserRepository
{
    public Task<AppUser?> GetByIdAsync(ObjectId userId, CancellationToken cancellationToken);
    public Task<AppUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken);
    public Task<ObjectId?> GetIdByUserNameAsync(string userName, CancellationToken cancellationToken);
    public Task<string?> GetKnownAsByUserNameAsync(string userName, CancellationToken cancellationToken);
    public Task<string?> GetGenderByIdAsync(ObjectId userId, CancellationToken cancellationToken);

    public Task<UpdateResult?> UpdateUserAsync(UserUpdateDto userUpdateDto, ObjectId userId, CancellationToken cancellationToken);
    public Task<PhotoUploadStatus> UploadPhotoAsync(IFormFile file, ObjectId userId, CancellationToken cancellationToken);
    public Task<UpdateResult?> SetMainPhotoAsync(ObjectId userId, string photoUrlIn, CancellationToken cancellationToken);
    public Task<UpdateResult?> DeletePhotoAsync(ObjectId userId, string? urlIn, CancellationToken cancellationToken);
}