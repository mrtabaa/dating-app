namespace api.Services;

public class PhotoService : IPhotoService
{
    #region Constructor
    private readonly IPhotoModifyService _photoModifyService;

    public PhotoService(IPhotoModifyService photoModifyService)
    {
        _photoModifyService = photoModifyService;
    }
    #endregion

    public async Task<IEnumerable<PhotoAddResultsDto>> AddPhoto(IEnumerable<IFormFile> formFiles, string userId)
    {
        List<PhotoAddResultsDto> photoAddResults = new();

        // copy file/s to the folder
        foreach (IFormFile formFile in formFiles)
        {
            if (formFile.Length > 0)
            {
                string? filePath = string.Empty;

                #region Resize and Create Images
                // filePath = await _photoModifyService.ResizeImageByScale(formFile, userId);

                // filePath = await _photoModifyService.ResizeByPixel(formFile, userId, 500, 1000);

                filePath = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 128);
                // filePath = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 256);
                filePath = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 512);
                // filePath = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 1024);

                // filePath = await _photoModifyService.CropAndSave(formFile, userId, 1000, 1200);
                #endregion Resize and Create Images

                #region if saving the original file
                // string filePath = Path.Combine(uploadsFolder + uniqueFileName);

                // using (var stream = new FileStream(filePath, FileMode.Create))
                // {
                //     await formFile.CopyToAsync(stream);
                // }
                #endregion

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
