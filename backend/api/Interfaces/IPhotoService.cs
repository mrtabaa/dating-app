namespace api.Interfaces;

public interface IPhotoService
{
    public Task<string[]?> AddPhotoToBlob(IFormFile file, string? userId, CancellationToken cancellationToken);
    public IEnumerable<Photo>? ConvertAllPhotosToBlobLinkFormat(IEnumerable<Photo> photo);
    public Task<bool> DeletePhotoFromBlob(Photo photo, CancellationToken cancellationToken);
}
