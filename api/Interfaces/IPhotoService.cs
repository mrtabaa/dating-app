namespace api.Interfaces;

public interface IPhotoService
{
    public Task<IEnumerable<PhotoAddResultsDto>> AddPhoto(IEnumerable<IFormFile> file, string userId);
    // public Task<string?> DeletePhoto(string url);
}
