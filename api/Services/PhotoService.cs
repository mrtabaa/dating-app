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

    public async Task<IEnumerable<Photo>?> AddPhotosToDisk(IEnumerable<IFormFile> formFiles, string userId, List<Photo> userPhotos)
    {
        // copy file/s to the folder
        foreach (IFormFile formFile in formFiles)
        {
            if (formFile.Length > 0)
            {
                #region Resize and/or Store Images to Disk
                // string? filePath_original = await _photoModifyService.SaveImage(formFile, userId, 4); // 4 is from the service's ENUM

                // string? filePath_scale = await _photoModifyService.ResizeImageByScale(formFile, userId);

                // string? filePath_var = await _photoModifyService.ResizeByPixel(formFile, userId, 500, 1000);

                #region ResizeByPixel_Square
                string? filePath_crop_sq = string.Empty;
                string? filePath_128_sq = string.Empty;
                string? filePath_256_sq = string.Empty;
                string? filePath_512_sq = string.Empty;
                string? filePath_1024_sq = string.Empty;

                // prevent storing files on disk if no resize is needed/performed for a larger size.
                switch (formFile.Length)
                {
                    case < 128 * 128:
                        filePath_crop_sq = await _photoModifyService.CropWithOriginalSideAndSave_Square(formFile, userId);
                        filePath_1024_sq = filePath_512_sq = filePath_256_sq = filePath_128_sq = filePath_crop_sq;
                        break;
                    case < 256 * 256:
                        filePath_128_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 128);
                        filePath_1024_sq = filePath_512_sq = filePath_256_sq = filePath_128_sq;
                        break;
                    case < 512 * 512:
                        filePath_128_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 128);
                        filePath_256_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 256);
                        filePath_1024_sq = filePath_512_sq = filePath_256_sq;
                        break;
                    case < 1024 * 1024:
                        filePath_128_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 128);
                        filePath_256_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 256);
                        filePath_512_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 512);
                        filePath_1024_sq = filePath_512_sq;
                        break;
                    default:
                        filePath_128_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 128);
                        filePath_256_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 256);
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

                #region Create Photos list to save their URLs to db
                // generate "storage/photos/user-id/resize-pixel-square/128x128/my-photo.jpg"
                filePath_128_sq = filePath_128_sq.Split(wwwRootUrl)[1];
                filePath_512_sq = filePath_512_sq.Split(wwwRootUrl)[1];
                filePath_1024_sq = filePath_1024_sq.Split(wwwRootUrl)[1];

                Photo photo;
                if (!userPhotos.Any()) // if user's album is empty set Main: true
                {
                    photo = new Photo(
                        Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                        Url_128: filePath_128_sq,
                        Url_512: filePath_512_sq,
                        Url_1024: filePath_1024_sq,
                        IsMain: true
                    );
                }
                else
                {
                    photo = new Photo(
                        Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                        Url_128: filePath_128_sq,
                        Url_512: filePath_512_sq,
                        Url_1024: filePath_1024_sq,
                        IsMain: false
                    );
                }
                userPhotos.Add(photo);
            }
            #endregion
        }

        return userPhotos;
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
