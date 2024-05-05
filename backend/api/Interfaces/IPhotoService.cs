namespace api.Interfaces;

public interface IPhotoService
{
    public Task<string[]?> AddPhotoToBlob(IFormFile file, string? userId, CancellationToken cancellationToken);
    public Photo? ConvertPhotoToBlobLinkWithSas(Photo photo);
    public IEnumerable<Photo>? ConvertAllPhotosToBlobLinkWithSas(IEnumerable<Photo> photo);
    public Task<bool> DeletePhotoFromBlob(Photo photo, CancellationToken cancellationToken);
}
