namespace api.Interfaces;
public interface IUserRepository
{
    public Task<List<MemberDto?>> GetUsersAsync(CancellationToken cancellationToken);

    public Task<MemberDto?> GetUserByIdAsync(string? userId, CancellationToken cancellationToken);

    public Task<MemberDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);

    public Task<UpdateResult?> UpdateUserAsync(MemberUpdateDto memberUpdateDto, string? Id, CancellationToken cancellationToken);

    public Task<DeleteResult?> DeleteUserAsync(string? userId, CancellationToken cancellationToken);

    public Task<Photo?> UploadPhotosAsync(IFormFile file, string? userId, CancellationToken cancellationToken);

    public Task<UpdateResult?> DeleteOnePhotoAsync(string? userId, string? urlIn, CancellationToken cancellationToken);

    public Task<UpdateResult?> SetMainPhotoAsync(string? userId, string photoUrlIn, CancellationToken cancellationToken);

}