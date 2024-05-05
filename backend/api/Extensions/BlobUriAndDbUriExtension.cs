using Azure.Storage;
using Azure.Storage.Sas;

namespace api.Extensions;

public static class BlobUriAndDbUriExtension
{
    /// <summary>
    /// Convert blobUri format to dbUri format.
    /// From 
    ///     https://dablobstorage.blob.core.windows.net/photos/66377887965c65551fe5bd10/resize-pixel-square/165x165/8f4c9a41-7837-4a00-afc9-b3440d4e55b1_pexels-adrienn-1542085.webp?sv=2023-11-03&st=2024-04-05T13%3A54%3A04Z&se=2024-05-06T13%3A54%3A04Z&sr=b&sp=rl&sig=BnRgSYl6BTYYoir23Ebc3GCdcyK0888OeiseyqD9QnU%3D
    /// To
    ///     66377887965c65551fe5bd10/resize-pixel-square/165x165/8f4c9a41-7837-4a00-afc9-b3440d4e55b1_pexels-adrienn-1542085.webp
    /// </summary>
    /// <param name="blobUri"></param>
    /// <returns>string dbUri. If blobUri is null return null</returns>
    public static string? ConvertBlobUriToDbUri(string? blobUri, string containerName)
    {
        if (blobUri is null) return null;

        string? imagePath = blobUri?.Substring(blobUri.IndexOf($"{containerName}/") + $"{containerName}/".Length);
        return imagePath?.Substring(0, imagePath.IndexOf("?sv="));
    }

    /// <summary>
    /// Gets a fileName (blobName), generate a fresh SasToken, form a full link of the blob, and return it. 
    /// private function used in ConvertPhotoNameToBlobLinkFormat()
    /// </summary>
    /// <param name="blobName"></param>
    /// <returns>A blob full link</returns>
    public static string? ConvertDbUriToBlobUriWithSas(string blobName, IConfiguration configuration, BlobContainerClient blobContainerClient)
    {
        #region Get the StorageConnectionString value
        string? connectionString = configuration.GetValue<string>("StorageConnectionString");
        if (string.IsNullOrEmpty(connectionString)) return null;

        // Parse the connection string
        var connectionStringParts = new Dictionary<string, string>();

        // Split the connection string into parts
        foreach (var part in connectionString.Split(';'))
        {
            var keyValue = part.Split(['='], 2);
            if (keyValue.Length == 2)
            {
                connectionStringParts[keyValue[0]] = keyValue[1];
            }
        }

        // Extract the account name and key
        connectionStringParts.TryGetValue("AccountName", out string? accountName);
        connectionStringParts.TryGetValue("AccountKey", out string? accountKey);

        #endregion Get the StorageConnectionString value

        // Create a SAS token that's valid for one hour
        BlobSasBuilder blobSasBuilder = new()
        {
            BlobContainerName = blobContainerClient.Name,
            BlobName = blobName,
            Resource = "b", // for "c", do NOT provide the BlobName
            StartsOn = DateTimeOffset.UtcNow.AddMonths(-1),
            ExpiresOn = DateTimeOffset.UtcNow.AddDays(1),
        };
        blobSasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.List);

        if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(accountKey)) return null;

        // Generate the SAS token using the BlobServiceClient's key
        string sasToken = blobSasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(accountName, accountKey)).ToString();

        return $"https://{accountName}.blob.core.windows.net/{blobContainerClient.Name}/{blobName}?{sasToken}";
    }
}
