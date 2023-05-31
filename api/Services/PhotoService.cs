using api.Extensions.Validations;

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

    public async Task<IEnumerable<PhotoAddResults>> AddPhoto(IEnumerable<IFormFile> files, string? userId)
    {
        List<PhotoAddResults> photoAddResults = new();

        // find or create folder path
        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Storage/Photos/" + userId + "/");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // copy file/s to the folder
        foreach (IFormFile file in files)
        {
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder + uniqueFileName);

            photoAddResults.Add(new PhotoAddResults(
                Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                Url: filePath
            ));

            await file.CopyToAsync(new FileStream(filePath, FileMode.Create));
        }

        return photoAddResults;
    }

    // public async Task<string?> DeletePhoto(string url)
    // {
    //     throw new NotImplementedException();
    // }
}
