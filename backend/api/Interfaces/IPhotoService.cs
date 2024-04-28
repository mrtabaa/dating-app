namespace api.Interfaces;

public interface IPhotoService
{
    public Task<string[]?> AddPhotoToBlob(IFormFile file, string? userId, CancellationToken cancellationToken);
    public Task<bool> DeletePhotoFromBlob(Photo photo, CancellationToken cancellationToken);
}
