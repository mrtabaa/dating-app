using Azure.Storage.Blobs.Models;

namespace api.Controllers.Helpers;

[Route("api/delete-all-photo-blobs")]
public class DeleteAllPhotoBlobsController(BlobServiceClient blobServiceClient) : BaseApiController
{
    private readonly BlobContainerClient _blobContainerClient = blobServiceClient.GetBlobContainerClient("photos");

    [HttpDelete]
    public async Task<ActionResult> DeleteAllBlobs()
    {
        await foreach (BlobItem photo in _blobContainerClient.GetBlobsAsync(BlobTraits.None, BlobStates.None))
        {
            // Get a reference to the blob
            BlobClient blobClient = _blobContainerClient.GetBlobClient(photo.Name);

            // Delete the blob if it exists
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        return Ok("All blobs got deleted.");
    }
}