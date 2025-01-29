namespace api.Interfaces;

public interface IUserRepository
{
    public Task<AppUser?> GetByIdAsync(ObjectId userId, CancellationToken cancellationToken);
    public Task<AppUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken);

    public Task<OperationResult<AppUser>> GetByRefreshTokenAsync(
        string refreshToken, CancellationToken cancellationToken
    );

    public Task<OperationResult<ObjectId>> GetIdByUserNameAsync(string userName, CancellationToken cancellationToken);
    public Task<string?> GetUserNameByIdentifierHashAsync(string identifierHash, CancellationToken cancellationToken);
    public Task<string?> GetKnownAsByUserNameAsync(string userName, CancellationToken cancellationToken);
    public Task<string?> GetGenderByIdAsync(ObjectId userId, CancellationToken cancellationToken);

    public Task<OperationResult> UpdateUserAsync(
        UserUpdateDto userUpdateDto, ObjectId userId, CancellationToken cancellationToken
    );

    public Task<OperationResult<Photo>> UploadPhotoAsync(
        IFormFile file, ObjectId userId, CancellationToken cancellationToken
    );

    public Task<OperationResult> SetMainPhotoAsync(
        ObjectId userId, string photoUrlIn, CancellationToken cancellationToken
    );

    public Task<string?> GetProfilePhotoUrlBlobAsync(ObjectId userId, CancellationToken cancellationToken);

    public Task<PhotoDeleteResponse> DeletePhotoAsync(
        ObjectId userId, string? urlIn, CancellationToken cancellationToken
    );
}