namespace api.Interfaces;

public interface IPhotoService
{
    public Task<IEnumerable<Photo>?> AddPhotos(IEnumerable<IFormFile> file, string userId, List<Photo> userPhotos);
    // public Task<string?> DeletePhoto(string url);
}
