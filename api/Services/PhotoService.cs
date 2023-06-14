namespace api.Services;

public class PhotoService : IPhotoService
{
    #region Constructor
    private readonly IPhotoModifySaveService _photoModifyService;
    private readonly ILogger<IPhotoModifySaveService> _logger;

    public PhotoService(IPhotoModifySaveService photoModifyService, ILogger<IPhotoModifySaveService> logger)
    {
        _logger = logger;
        _photoModifyService = photoModifyService;
    }
    #endregion

    public async Task<IEnumerable<Photo>?> AddPhotos(IEnumerable<IFormFile> formFiles, string userId, List<Photo> userPhotos)
    {
        // copy file/s to the folder
        foreach (IFormFile formFile in formFiles)
        {
            if (formFile.Length > 0)
            {
                #region Resize and Store Images on Disk
                // filePath_scale = await _photoModifyService.ResizeImageByScale(formFile, userId);

                // string? filePath_var = await _photoModifyService.ResizeByPixel(formFile, userId, 500, 1000);

                string? filePath_128_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 128);
                // string? filePath_256_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 256);
                string? filePath_512_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 512);
                // string? filePath_1024_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 1024);

                string? filePath_crop = await _photoModifyService.CropAndSave(formFile, userId, 1000, 1200);

                #region if saving the original file
                // string filePath = Path.Combine(uploadsFolder + uniqueFileName);

                // using (var stream = new FileStream(filePath, FileMode.Create))
                // {
                //     await formFile.CopyToAsync(stream);
                // }
                #endregion


                // if conversion fails
                if (filePath_128_sq is null || filePath_512_sq is null || filePath_crop is null)
                {
                    _logger.LogError("Photo addition failed. e.g. Height/Weight input is larger than the original image size.");
                    return null;
                }
                #endregion Resize and Create Images on Disk

                #region Create Photos list to save to db
                // if user's album is empty

                Photo photo;
                if (!userPhotos.Any())
                {
                    photo = new Photo(
                        Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                        Url_128: filePath_128_sq,
                        Url_512: filePath_512_sq,
                        IsMain: true
                    );
                }
                else
                {
                    photo = new Photo(
                        Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                        Url_128: filePath_128_sq,
                        Url_512: filePath_512_sq,
                        IsMain: false
                    );
                }
                userPhotos.Add(photo);
            }
            #endregion
        }

        return userPhotos;
    }
}
