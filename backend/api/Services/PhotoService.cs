namespace api.Services;

public class PhotoService(IPhotoModifySaveService _photoModifyService, ILogger<IPhotoModifySaveService> _logger) : PhotoStandardSize, IPhotoService
{
    #region Constructor and variables

    #endregion

    /// <summary>
    /// ADD PHOTO TO BLOB
    /// Store photos on blob by creating a folder based on fileName, userId, size, crop, etc. Each user will have a folder named after their db _Id.
    /// Resize and square image with 165px(navbar & thumbnail), 256px(card).
    /// Scale image to a ~300kb max size for the enlarged gallery photo.
    /// Store photo address in db as "user-id/resize-pixel-square/128x128/GUID_my-photo.jpg"
    /// 
    /// DELETE PHOTO FROM DISK
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="userId"></param>
    /// <returns>Array of filePaths</returns>
    public async Task<string[]?> AddPhotoToBlob(IFormFile formFile, string? userId, CancellationToken cancellationToken)
    {
        // copy file/s to the folder
        if (formFile.Length > 0 && !string.IsNullOrEmpty(userId))
        {
            #region Resize and/or Store Images to Blob
            string? filePath_165_sq; // navbar & thumbnail
            string? filePath_256_sq; // card
            string? filePath_enlarged; // enlarged photo up to ~300kb

            filePath_165_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 165, cancellationToken); // navbar & thumbnail
            filePath_256_sq = await _photoModifyService.ResizeByPixel_Square(formFile, userId, 256, cancellationToken); // card
            filePath_enlarged = await _photoModifyService.ResizeImageByScale(formFile, userId, (int)DimensionsEnum._4_3_800x600, cancellationToken); // enlarged photo

            // if image processing fails
            if (filePath_165_sq is null || filePath_256_sq is null || filePath_enlarged is null)
            {
                _logger.LogError("Photo addition failed. The returned filePath is null which is not allowed.");
                return null;
            }
            #endregion Resize and Create Images to Blob

            #region return the result if success
            return [
                filePath_165_sq,
                filePath_256_sq,
                filePath_enlarged
            ];
            #endregion
        }

        return null;
    }

    /// <summary>
    /// Delete all files of the requested photo to be deleted.
    /// </summary>
    /// <param name="photo"></param>
    /// <returns>bool</returns>
    public async Task<bool> DeletePhotoFromBlob(Photo photo, CancellationToken cancellationToken)
    {
        List<string> photoPaths = [];

        photoPaths.Add(photo.Url_165);
        photoPaths.Add(photo.Url_256);
        photoPaths.Add(photo.Url_enlarged);

        foreach (string photoPath in photoPaths)
        {
            if (File.Exists(photoPath))
            {
                // Delete the file on a background thread and await the task
                await Task.Run(() => File.Delete(photoPath));
            }
            else
                return false;
        }

        return true;
    }
}