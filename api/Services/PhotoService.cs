namespace api.Services;

public class PhotoService : IPhotoService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<PhotoService> _logger;

    public PhotoService(IWebHostEnvironment webHostEnvironment, ILogger<PhotoService> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    public async Task<string?> AddPhoto(IFormFile photo, string? userId)
    {
        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Storage/Photos/" + userId + "/");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        string uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
        string filePath = Path.Combine(uploadsFolder + uniqueFileName);

        await photo.CopyToAsync(new FileStream(filePath, FileMode.Create));

        return filePath;
    }

    public async Task<string?> DeletePhoto(string url)
    {
        throw new NotImplementedException();
    }
}
