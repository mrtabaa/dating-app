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

    public async Task<IEnumerable<PhotoAddResultsDto>> AddPhoto(IEnumerable<IFormFile> formFiles, string? userId)
    {
        List<PhotoAddResultsDto> photoAddResults = new();

        // find path OR create folder if doesn't exist by userId
        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Storage/Photos/" + userId + "/");
        if (!Directory.Exists(uploadsFolder)) // create folder
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // copy file/s to the folder
        foreach (IFormFile formFile in formFiles)
        {
            if (formFile.Length > 0)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + formFile.FileName;
                string filePath = Path.Combine(uploadsFolder + uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                }

                #region Resize and Create Images
                // await ResizePhotoExtensions.ResizeCreateImage_128x128(uploadsFolder, filePath, uniqueFileName);
                await ResizePhotoExtensions.ResizeCreateImage_256x256(uploadsFolder, filePath, uniqueFileName);
                // await ResizePhotoExtensions.ResizeCreateImage_512x512(uploadsFolder, filePath, uniqueFileName);
                // await ResizePhotoExtensions.ResizeCreateImage_1024x1024(uploadsFolder, filePath, uniqueFileName);

                await ResizePhotoExtensions.ResizeCreateImageByScale(uploadsFolder, filePath, uniqueFileName, formFile.Length);
                #endregion Resize and Create Images

                photoAddResults.Add(new PhotoAddResultsDto(
                    Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                    Url: filePath
                ));
            }
        }

        return photoAddResults;
    }

    // public async Task<string?> DeletePhoto(string url)
    // {
    //     throw new NotImplementedException();
    // }
}
