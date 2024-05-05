using Azure.Storage.Blobs.Models;

namespace api.Services;

public class PhotoService(
    IPhotoModifySaveService _photoModifyService,
    BlobServiceClient _blobServiceClient,
    IConfiguration _configuration,
    ILogger<IPhotoModifySaveService> _logger) : PhotoStandardSize, IPhotoService
{
    private readonly BlobContainerClient _blobContainerClient = _blobServiceClient.GetBlobContainerClient("photos");

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
                _logger.LogError("Photo addition failed. The returned filePath is null which is not allowed. Photo is not uploaded on Azure storage");
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
    /// Gets a list of appUser.Photos from db and completes all their links to the full blob format.
    /// </summary>
    /// <param name="photos"></param>
    /// <returns>'IEnumerable<Photo> photos on success OR 'null' on fail</returns>
    public Photo? ConvertPhotoToBlobLinkWithSas(Photo? photo)
    {
        if (photo is null) return null;

        string? url_165 = BlobUriDbUriExtension.ConvertDbUriToBlobUriWithSas(photo.Url_165, _configuration, _blobContainerClient);
        string? url_256 = BlobUriDbUriExtension.ConvertDbUriToBlobUriWithSas(photo.Url_256, _configuration, _blobContainerClient);
        string? url_enlarged = BlobUriDbUriExtension.ConvertDbUriToBlobUriWithSas(photo.Url_enlarged, _configuration, _blobContainerClient);

        // Link conversion failed
        if (string.IsNullOrEmpty(url_165) || string.IsNullOrEmpty(url_256) || string.IsNullOrEmpty(url_enlarged))
            return null;

        // Create Photo on link conversion success
        return new Photo
        {
            Schema = photo.Schema,
            Url_165 = url_165,
            Url_256 = url_256,
            Url_enlarged = url_enlarged,
            IsMain = photo.IsMain
        };
    }

    /// <summary>
    /// Gets a list of appUser.Photos from db and completes all their links to the full blob format.
    /// </summary>
    /// <param name="photos"></param>
    /// <returns>'IEnumerable<Photo> photos on success OR 'null' on fail</returns>
    public IEnumerable<Photo>? ConvertAllPhotosToBlobLinkWithSas(IEnumerable<Photo> photos)
    {
        List<Photo> blobPhotos = [];

        foreach (Photo photo in photos)
        {
            string? url_165 = BlobUriDbUriExtension.ConvertDbUriToBlobUriWithSas(photo.Url_165, _configuration, _blobContainerClient);
            string? url_256 = BlobUriDbUriExtension.ConvertDbUriToBlobUriWithSas(photo.Url_256, _configuration, _blobContainerClient);
            string? url_enlarged = BlobUriDbUriExtension.ConvertDbUriToBlobUriWithSas(photo.Url_enlarged, _configuration, _blobContainerClient);

            // Link conversion failed
            if (string.IsNullOrEmpty(url_165) || string.IsNullOrEmpty(url_256) || string.IsNullOrEmpty(url_enlarged))
                return null;

            // Create Photo on link conversion success
            blobPhotos.Add(
                new Photo
                {
                    Schema = photo.Schema,
                    Url_165 = url_165,
                    Url_256 = url_256,
                    Url_enlarged = url_enlarged,
                    IsMain = photo.IsMain
                }
            );
        }

        return blobPhotos;
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
            // Get a reference to a blob
            BlobClient blobClient = _blobContainerClient.GetBlobClient(photoPath);

            // Delete the blob and its snapshots if exists
            if (!await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, null, cancellationToken))
                return false; // if blob doesn't exist or deletion fails
        }

        return true;
    }
}