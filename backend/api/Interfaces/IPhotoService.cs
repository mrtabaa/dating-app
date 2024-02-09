namespace api.Interfaces;

public interface IPhotoService
{
    public Task<string[]?> AddPhotoToDisk(IFormFile file, string? userId);
    public Task<bool> DeletePhotoFromDisk(Photo photo);
}
