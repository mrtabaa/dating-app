namespace api.Interfaces;

public interface IPhotoService
{
    public Task<string?> AddPhoto(IFormFile file, string? userId);
    public Task<string?> DeletePhoto(string url);
}
