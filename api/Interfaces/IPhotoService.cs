namespace api.Interfaces;

public interface IPhotoService
{
    public Task<string[]?> AddPhotoToDisk(IFormFile file, string userId);
    public bool DeletePhotoFromDisk(IEnumerable<string> filePaths);
}
