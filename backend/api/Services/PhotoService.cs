using Azure.Storage.Blobs.Models;
using image_processing.Helpers;
using image_processing.Interfaces;

namespace api.Services;

public class PhotoService(
    IPhotoModifySaveService photoModifyService,
    BlobServiceClient blobServiceClient,
    IConfiguration configuration,
    ILogger<IPhotoModifySaveService> logger) : PhotoStandardSize, IPhotoService
{
    private readonly BlobContainerClient _blobContainerClient = blobServiceClient.GetBlobContainerClient("photos");

    /// <summary>
    ///     ADD PHOTO TO BLOB
    ///     Store photos on blob by creating a folder based on fileName, userId, size, crop, etc. Each user will have a folder
    ///     named after their db _Id.
    ///     Resize and square image with 165px(navbar & thumbnail), 256px(card).
    ///     Scale image to a ~300kb max size for the enlarged gallery photo.
    ///     Store photo address in db as "user-id/resize-pixel-square/128x128/GUID_my-photo.jpg"
    ///     DELETE PHOTO FROM DISK
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Array of filePaths</returns>
    public async Task<string[]?> AddPhotoToBlob(IFormFile formFile, string? userId, CancellationToken cancellationToken)
    {
        // copy file/s to the folder
        if (formFile.Length <= 0 || string.IsNullOrEmpty(userId)) return null;

        #region Resize and/or Store Images to Blob

        #region return the result if success

        string? filePath165Sq = await photoModifyService.ResizeByPixel_Square(formFile, userId, 165, cancellationToken); // navbar & thumbnail
        // navbar & thumbnail
        string? filePath256Sq = await photoModifyService.ResizeByPixel_Square(formFile, userId, 256, cancellationToken); // card
        // card
        string? filePathEnlarged = await photoModifyService.ResizeImageByScale(formFile, userId, (int)DimensionsEnum._4_3_800x600, cancellationToken); // enlarged photo
        // enlarged photo up to ~300kb
        // if image processing fails
        if (!(filePath165Sq is null || filePath256Sq is null || filePathEnlarged is null))
        {
            return
            [
                filePath165Sq,
                filePath256Sq,
                filePathEnlarged
            ];
        }

        #endregion

        logger.LogError("Photo addition failed. The returned filePath is null which is not allowed. Photo is not uploaded on Azure storage");
        return null;

        #endregion Resize and Create Images to Blob
    }

    /// <summary>
    ///     Delete all photo versions/sizes to be deleted.
    /// </summary>
    /// <param name="photo"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>bool</returns>
    public async Task<bool> DeletePhotoFromBlob(Photo photo, CancellationToken cancellationToken)
    {
        List<string> photoPaths =
        [
            photo.Url165,
            photo.Url256,
            photo.UrlEnlarged
        ];

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

    /// <summary>
    ///     Gets a photo URL and convert it to the full blob format.
    /// </summary>
    /// <param name="url"></param>
    /// <returns>'string on success OR 'null' on fail</returns>
    public string? ConvertPhotoUrlToBlobLinkWithSas(string? url)
    {
        if (string.IsNullOrEmpty(url)) return null;

        return BlobUriAndDbUriExtension.ConvertDbUriToBlobUriWithSas(url, configuration, _blobContainerClient);
    }

    /// <summary>
    ///     Gets a list of appUser.Photos from db and completes all their links to the full blob format.
    /// </summary>
    /// <param name="photo"></param>
    /// <returns Photo="photos on success OR &apos;null&apos; on fail">'IEnumerable</returns>
    public Photo? ConvertPhotoToBlobLinkWithSas(Photo? photo)
    {
        if (photo is null) return null;

        string? url165 = BlobUriAndDbUriExtension.ConvertDbUriToBlobUriWithSas(photo.Url165, configuration, _blobContainerClient);
        string? url256 = BlobUriAndDbUriExtension.ConvertDbUriToBlobUriWithSas(photo.Url256, configuration, _blobContainerClient);
        string? urlEnlarged = BlobUriAndDbUriExtension.ConvertDbUriToBlobUriWithSas(photo.UrlEnlarged, configuration, _blobContainerClient);

        // Link conversion failed
        if (string.IsNullOrEmpty(url165) || string.IsNullOrEmpty(url256) || string.IsNullOrEmpty(urlEnlarged))
            return null;

        // Create Photo on link conversion success
        return new Photo
        {
            Schema = photo.Schema,
            Url165 = url165,
            Url256 = url256,
            UrlEnlarged = urlEnlarged,
            IsMain = photo.IsMain
        };
    }

    /// <summary>
    ///     Gets a list of appUser.Photos from db and completes all their links to the full blob format.
    /// </summary>
    /// <param name="photos"></param>
    /// <returns Photo="photos on success OR &apos;null&apos; on fail">'IEnumerable</returns>
    public IEnumerable<Photo> ConvertAllPhotosToBlobLinkWithSas(IEnumerable<Photo> photos)
    {
        List<Photo> blobPhotos = [];

        foreach (Photo photo in photos)
        {
            string url165 = BlobUriAndDbUriExtension.ConvertDbUriToBlobUriWithSas(photo.Url165, configuration, _blobContainerClient);
            string url256 = BlobUriAndDbUriExtension.ConvertDbUriToBlobUriWithSas(photo.Url256, configuration, _blobContainerClient);
            string urlEnlarged = BlobUriAndDbUriExtension.ConvertDbUriToBlobUriWithSas(photo.UrlEnlarged, configuration, _blobContainerClient);

            // Create Photo on link conversion success
            blobPhotos.Add(
                new Photo
                {
                    Schema = photo.Schema,
                    Url165 = url165,
                    Url256 = url256,
                    UrlEnlarged = urlEnlarged,
                    IsMain = photo.IsMain
                }
            );
        }

        return blobPhotos;
    }
}