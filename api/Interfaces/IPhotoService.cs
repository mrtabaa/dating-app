namespace api.Interfaces;

public interface IPhotoService
{
    public Task<string[]?> AddPhotosToDisk(IFormFile file, string userId);
    public bool DeletePhotoFromDisk(IEnumerable<string> filePaths);
}
