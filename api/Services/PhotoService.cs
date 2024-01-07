namespace api.Services;

public class PhotoService(IPhotoModifySaveService _photoModifyService, ILogger<IPhotoModifySaveService> _logger) : IPhotoService
{
    #region Constructor and variables

    const string wwwRootUrl = "wwwroot/";

    #endregion

    /// <summary>
    /// ADD PHOTO TO DISK
    /// Store photos on disk by creating a folder based on fileName, userId, size, crop, etc. Each user will have a folder named after their db _Id.
    /// Resize and square image with 165px(navbar & thumbnail), 256px(card).
    /// Scale image to a ~300kb max size for the enlarged gallery photo.
    /// Store photo address in db as "storage/photos/user-id/resize-pixel-square/128x128/my-photo.jpg"
    /// 
    /// DELETE PHOTO FROM DISK
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="userId"></param>
    /// <returns>ADD: array of filePaths. DELETE: boolean</returns>
    public async Task<string[]?> AddPhotoToDisk(IFormFile formFile, string userId)
    {
        // copy file/s to the folder
        if (formFile.Length > 0)
        {
            #region Resize and/or Store Images to Disk
            string? filePath_165_sq; // navbar & thumbnail
            string? filePath_256_sq; // card
            string? filePath_enlarged; // enlarged photo up to ~300kb

            filePath_165_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 165); // navbar & thumbnail
            filePath_256_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 256); // card
            filePath_enlarged = await _photoModifyService.ResizeImageByScale(formFile, userId); // enlarged photo

            // if conversion fails
            if (filePath_165_sq is null || filePath_256_sq is null || filePath_enlarged is null)
            {
                _logger.LogError("Photo addition failed. The returned filePath is null which is not allowed.");
                return null;
            }
            #endregion Resize and Create Images to Disk

            #region Create the photo URLs and return the result
            // // generate "storage/photos/user-id/resize-pixel-square/128x128/my-photo.jpg"
            return [
                filePath_165_sq.Split(wwwRootUrl)[1],
                filePath_256_sq.Split(wwwRootUrl)[1],
                filePath_enlarged.Split(wwwRootUrl)[1]
            ];
            #endregion
        }

        return null;
    }

    /// <summary>
    /// Delete all files of the requested photo to be deleted.
    /// </summary>
    /// <param name="filePaths"></param>
    /// <returns>bool</returns>
    public bool DeletePhotoFromDisk(IEnumerable<string> filePaths)
    {
        foreach (string filePath in filePaths)
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