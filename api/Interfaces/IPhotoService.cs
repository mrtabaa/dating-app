namespace api.Interfaces;

public interface IPhotoService
{
    public Task<IEnumerable<Photo>?> AddPhotosToDisk(IFormFile file, string userId, List<Photo> userPhotos);
    public bool DeletePhotoFromDisk(IEnumerable<string> filePaths);
}
