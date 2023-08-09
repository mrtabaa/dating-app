namespace api.Services;

public class PhotoService : IPhotoService
{
    #region Constructor and variables

    private readonly IPhotoModifySaveService _photoModifyService;
    private readonly ILogger<IPhotoModifySaveService> _logger;
    const string wwwRootUrl = "wwwroot/";

    public PhotoService(IPhotoModifySaveService photoModifyService, ILogger<IPhotoModifySaveService> logger)
    {
        _logger = logger;
        _photoModifyService = photoModifyService;
    }
    #endregion

    public async Task<string[]?> AddPhotosToDisk(IFormFile formFile, string userId)
    {
        // copy file/s to the folder
        if (formFile.Length > 0)
        {

            #region Resize and/or Store Images to Disk
            #region ResizeByPixel_Square

            string? filePath_128_sq;
            string? filePath_256_sq; // NOT USED in this app
            string? filePath_512_sq;
            string? filePath_1024_sq;
            // prevent storing files on disk if no resize is needed/performed for a larger size.
            switch (formFile.Length)
            {
                case < 128 * 128:
                    string? filePath_crop_sq = await _photoModifyService.CropWithOriginalSideAndSave_Square(formFile, userId);
                    filePath_1024_sq = filePath_512_sq = filePath_256_sq = filePath_128_sq = filePath_crop_sq;
                    break;
                // case < 256 * 256:
                //     filePath_128_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 128);
                //     filePath_1024_sq = filePath_512_sq = filePath_256_sq = filePath_128_sq;
                //     break;
                case < 512 * 512:
                    filePath_128_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 128);
                    filePath_256_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 256);
                    filePath_1024_sq = filePath_512_sq = filePath_256_sq;
                    break;
                case < 1024 * 1024:
                    filePath_128_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 128);
                    // filePath_256_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 256);
                    filePath_512_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 512);
                    filePath_1024_sq = filePath_512_sq;
                    break;
                default:
                    filePath_128_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 128);
                    // filePath_256_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 256);
                    filePath_512_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 512);
                    filePath_1024_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 1024);
                    break;
            }
            #endregion ResizeByPixel_Square

            // string? filePath_crop = await _photoModifyService.CropAndSave(formFile, userId, 1000, 1200);
            // string? filePath_crop_sq = await _photoModifyService.CropGivenSideAndSave_Square(formFile, userId, 512);
            // string? filePath_original_crop_sq = await _photoModifyService.CropWithOriginalSideAndSave_Square(formFile, userId);


            // if conversion fails
            if (filePath_128_sq is null || filePath_512_sq is null || filePath_1024_sq is null)
            {
                _logger.LogError("Photo addition failed. e.g. Height/Weight input is larger than the original image size.");
                return null;
            }
            #endregion Resize and Create Images to Disk

            #region Create the photo URLs and return the result
            // generate "storage/photos/user-id/resize-pixel-square/128x128/my-photo.jpg"
            return new string[]{
                filePath_128_sq.Split(wwwRootUrl)[1],
                filePath_512_sq.Split(wwwRootUrl)[1],
                filePath_1024_sq.Split(wwwRootUrl)[1]
            };
            #endregion
        }

        return null;
    }

    public bool DeletePhotoFromDisk(IEnumerable<string> filePaths)
    {
        // delete all 3 resolutions
        foreach (var filePath in filePaths)
        {
            if (File.Exists(wwwRootUrl + filePath))
            {
                File.Delete(wwwRootUrl + filePath);
            }
            else
                return false;
        }

        return true;
    }
}
