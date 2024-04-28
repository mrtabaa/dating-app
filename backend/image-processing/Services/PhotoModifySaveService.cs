using image_processing.Interfaces;
using image_processing.Helpers;
using SkiaSharp;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace image_processing.Services;

public class PhotoModifySaveService(IWebHostEnvironment webHostEnvironment, BlobServiceClient blobServiceClient) : PhotoStandardSize, IPhotoModifySaveService
{
    #region Vars
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
    private readonly BlobContainerClient _blobContainerClient = blobServiceClient.GetBlobContainerClient("photos");

    readonly string[] operations = ["resize-scale", "resize-pixel", "resize-pixel-square", "crop", "original"];

    private enum OperationName
    {
        ResizeByScale,
        ResizeByPixel,
        ResizeByPixelSquare,
        Crop,
        Original
    }
    #endregion Vars

    #region Resize Methods

    /// <summary>
    /// Reduce by Scaling to around 300k
    /// Skip if already less than 300k
    /// Save image to hard if successful.
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="userId"></param>
    /// <param name="standardSizeIndex"></param>
    /// <returns>filePath</returns>
    public async Task<string> ResizeImageByScale(IFormFile formFile, string userId, int standardSizeIndex, CancellationToken cancellationToken)
    {
        // performace
        if (formFile.Length < 300_000)
            return await SaveImageAsIs(formFile, userId, (int)OperationName.Original, cancellationToken); // return filePath

        using var binaryReader = new BinaryReader(formFile.OpenReadStream());
        // get image from formFile
        byte[]? imageData = binaryReader.ReadBytes((int)formFile.Length);

        // convert imageData to SKImage
        using SKImage skImageSource = SKImage.FromEncodedData(imageData);
        using SKBitmap bitmapSource = SKBitmap.FromImage(skImageSource);

        int width;
        int height;

        if (bitmapSource.Width > bitmapSource.Height)
        {
            width = dimensions[standardSizeIndex].Side1;
            height = dimensions[standardSizeIndex].Side2;
        }
        else if (bitmapSource.Width < bitmapSource.Height)
        {
            width = dimensions[standardSizeIndex].Side2;
            height = dimensions[standardSizeIndex].Side1;
        }
        else
        {
            width = dimensions[standardSizeIndex].Side2;
            height = dimensions[standardSizeIndex].Side2;
        }

        using SKBitmap bitmapResized = bitmapSource.Resize(new SKImageInfo(width, height), SKFilterQuality.None);

        using SKImage sKImageResized = SKImage.FromBitmap(bitmapResized);
        using SKData sKData = sKImageResized.Encode(SKEncodedImageFormat.Webp, 90);

        return await SaveImage(sKData, userId, formFile.FileName, (int)OperationName.ResizeByScale, cancellationToken);
    }

    /// <summary>
    /// Resize image based on the input sides.
    /// It does NOT keep the original image's scale if proportional input sides are not given. 
    /// SaveImageAsIs if either input side is larger than the actual image's coresponding side.
    /// Save image to hard if successful.
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="userId"></param>
    /// <param name="widthIn"></param>
    /// <param name="heightIn"></param>
    /// <returns></returns>
    public async Task<string> ResizeByPixel(IFormFile formFile, string userId, int widthIn, int heightIn, CancellationToken cancellationToken)
    {
        // do the job
        using var binaryReader = new BinaryReader(formFile.OpenReadStream());
        // get image from formFile
        byte[]? imageData = binaryReader.ReadBytes((int)formFile.Length);

        // convert imageData to SKImage
        using SKImage skImage = SKImage.FromEncodedData(imageData);

        #region 3 x 5 OR 3 x 3 original image sides
        // 4 & in => 3 x 3
        // 3 in => 3 x 3
        // Already square with min side
        // performance: if either input side is larger than the actual image's coresponding side
        if (widthIn >= skImage.Width || heightIn >= skImage.Height)
            return await SaveImageAsIs(formFile, userId, (int)OperationName.Original, cancellationToken);
        #endregion

        #region images with sides greater than input sides
        using SKBitmap sourceBitmap = SKBitmap.FromImage(skImage);

        // resize
        using SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(widthIn, heightIn), SKFilterQuality.High);

        using SKImage scaledImage = SKImage.FromBitmap(scaledBitmap);
        using SKData sKData = scaledImage.Encode(SKEncodedImageFormat.Webp, 90);

        return await SaveImage(sKData, userId, formFile.FileName, (int)OperationName.ResizeByPixel, widthIn, heightIn, cancellationToken);
        #endregion
    }

    /// <summary>
    /// Crop-square then resize a photo based on one given side.
    /// Skip operation if image is already square with original min side < sideIn.
    /// Save image to hard if successful.
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="userId"></param>
    /// <param name="sideIn"></param>
    /// <returns>filePath</returns>
    public async Task<string> ResizeByPixel_Square(IFormFile formFile, string userId, int sideIn, CancellationToken cancellationToken)
    {
        using var binaryReader = new BinaryReader(formFile.OpenReadStream());
        // get image from formFile
        byte[]? imageData = binaryReader.ReadBytes((int)formFile.Length);

        // convert imageData to SKImage
        using SKImage skImage = SKImage.FromEncodedData(imageData);

        #region 3 x 3 original image sides
        // 4 in => 3 x 3
        // 3 in => 3 x 3
        // Already square with min side
        if (skImage.Width == skImage.Height && sideIn >= skImage.Width)
            // save original file
            return await SaveImageAsIs(formFile, userId, (int)OperationName.Original, cancellationToken); // return filePath
        #endregion

        #region 3 x 5 original image sides 
        // 6 in => 3 x 3
        // 3 in => 3 x 3
        // sideIn >= original min side (e.g sideIn = 6)
        if (sideIn >= Math.Min(skImage.Width, skImage.Height))
            return await CropWithOriginalSide_Square(formFile, userId, cancellationToken);
        #endregion

        #region 3 x 5 original image sides 
        // 2 in => 2 x 2
        // sideIn < original min side of 3 (e.g sideIn = 2)
        // crop to square (3 x 3) first, then resize by 2
        int smallerSide = Math.Min(skImage.Width, skImage.Height);

        // crop
        SKImage? croppedImage = CropImageForResize(skImage, smallerSide, smallerSide);

        // resize
        using SKBitmap croppedBitmap = SKBitmap.FromImage(croppedImage);
        using SKBitmap scaledBitmap = croppedBitmap.Resize(new SKImageInfo(sideIn, sideIn), SKFilterQuality.High);

        using SKImage scaledImage = SKImage.FromBitmap(scaledBitmap);
        using SKData sKData = scaledImage.Encode(SKEncodedImageFormat.Webp, 90);

        return await SaveImage(sKData, userId, formFile.FileName, (int)OperationName.ResizeByPixelSquare, sideIn, sideIn, cancellationToken);
        #endregion
    }

    #endregion Resize Methods 

    #region Crop Methods

    /// <summary>
    /// Crop image based on the input sides.
    /// Skip operation and save original image if both inputs are larger than the original image sides. Save the original image. 
    /// If either inputs are larger than the coresponding original image's width or height, keep the original side's size, then apply cropping.
    /// Save image to hard if successful.
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="userId"></param>
    /// <param name="widthIn"></param>
    /// <param name="heightIn"></param>
    /// <returns></returns>
    public async Task<string> Crop(IFormFile formFile, string userId, int widthIn, int heightIn, CancellationToken cancellationToken)
    {
        using var binaryReader = new BinaryReader(formFile.OpenReadStream());
        // get image from formFile
        byte[]? imageData = binaryReader.ReadBytes((int)formFile.Length);

        // convert imageData to SKImage
        using SKImage skImage = SKImage.FromEncodedData(imageData);

        #region Inputs are larger than original sides
        /// 3 x 5 or 3 x 3 image original sides
        // 4 in 6 in => 3 x 5
        // 3 in 5 in => 3 x 5
        if (widthIn <= skImage.Width && heightIn <= skImage.Height)
            return await SaveImageAsIs(formFile, userId, (int)OperationName.Original, cancellationToken);
        #endregion

        #region One or both inputs are smaller than the image original's either side
        ///// keep the smaller original side and reduce the other side by the input
        ///
        /// 3 x 5 or 3 x 3 image original sides

        // 4 in 4 in => 3 x 4
        // 3 in 4 in => 3 x 4
        if (widthIn >= skImage.Width && heightIn < skImage.Height)
        {
            widthIn = skImage.Width; // apply the image's width instead
        }
        // 2 in 6 in => 2 x 5
        // 2 in 5 in => 2 x 5
        if (widthIn < skImage.Width && heightIn >= skImage.Height)
        {
            heightIn = skImage.Height; // apply the image's height instead
        }

        // find the center
        int centerX = skImage.Width / 2;
        int centerY = skImage.Height / 2;

        // find the start points
        int startX = centerX - widthIn / 2;
        int startY = centerY - heightIn / 2;

        // crop image
        SKImage croppedImage = skImage.Subset(SKRectI.Create(startX, startY, widthIn, heightIn));
        using SKData sKData = croppedImage.Encode(SKEncodedImageFormat.Webp, 90);

        return await SaveImage(sKData, userId, formFile.FileName, (int)OperationName.Crop, widthIn, heightIn, cancellationToken);
        #endregion
    }

    /// <summary>
    /// Crop and square image based on one input side.
    /// Save image as-is and skip Crop if the given side is larger than the image size and it's already square.
    /// Save image to hard if successful.
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="userId"></param>
    /// <param name="sideIn"></param>
    /// <returns></returns>
    public async Task<string> Crop_Square(IFormFile formFile, string userId, int sideIn, CancellationToken cancellationToken)
    {
        using var binaryReader = new BinaryReader(formFile.OpenReadStream());
        // get image from formFile
        byte[]? imageData = binaryReader.ReadBytes((int)formFile.Length);

        // convert imageData to SKImage
        using SKImage skImage = SKImage.FromEncodedData(imageData);

        // if the given side is larger than the image size and it's already square
        if (sideIn > skImage.Width && skImage.Width == skImage.Height)
            return await SaveImageAsIs(formFile, userId, (int)OperationName.Original, cancellationToken);
        else
        {
            // find the center
            int centerX = skImage.Width / 2;
            int centerY = skImage.Height / 2;

            // find the start points
            int startX = centerX - sideIn / 2;
            int startY = centerY - sideIn / 2;

            // crop image
            SKImage croppedImage = skImage.Subset(SKRectI.Create(startX, startY, sideIn, sideIn));
            using SKData sKData = croppedImage.Encode(SKEncodedImageFormat.Webp, 90);

            return await SaveImage(sKData, userId, formFile.FileName, (int)OperationName.Crop, sideIn, sideIn, cancellationToken);
        }
    }

    /// <summary>
    /// Crop the image based on the smaller side of the image.
    /// Save as-is and skip Crop if it's already square.
    /// Save image to hard if successful.
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<string> CropWithOriginalSide_Square(IFormFile formFile, string userId, CancellationToken cancellationToken)
    {
        using var binaryReader = new BinaryReader(formFile.OpenReadStream());
        // get image from formFile
        byte[]? imageData = binaryReader.ReadBytes((int)formFile.Length);

        // convert imageData to SKImage
        using SKImage skImage = SKImage.FromEncodedData(imageData);

        //performance: Skip, already square.
        if (skImage.Width == skImage.Height)
            return await SaveImageAsIs(formFile, userId, (int)OperationName.Original, cancellationToken);


        int side = Math.Min(skImage.Width, skImage.Height);

        // find the center
        int centerX = skImage.Width / 2;
        int centerY = skImage.Height / 2;

        // find the start points
        int startX = centerX - side / 2;
        int startY = centerY - side / 2;

        // crop image
        SKImage croppedImage = skImage.Subset(SKRectI.Create(startX, startY, side, side));
        using SKData sKData = croppedImage.Encode(SKEncodedImageFormat.Webp, 90);

        return await SaveImage(sKData, userId, formFile.FileName, (int)OperationName.Crop, side, side, cancellationToken);
    }

    /// <summary>
    /// Crops SKImage for use in this class only.
    /// </summary>
    /// <param name="sKImage"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns>SKImage</returns>
    private SKImage CropImageForResize(SKImage sKImage, int width, int height)
    {
        // find the center
        int centerX = sKImage.Width / 2;
        int centerY = sKImage.Height / 2;

        // find the start points
        int startX = centerX - width / 2;
        int startY = centerY - height / 2;

        return sKImage.Subset(SKRectI.Create(startX, startY, width, height));
    }

    #endregion Crop Methods

    #region Save Methods
    /// <summary>
    /// Save image on the disk with inputs of width and height. 
    /// The path is generated by combining: userId + operation + width + x + height + Guid generated uniqueFileName.
    /// Used in this class only. 
    /// </summary>
    /// <param name="sKData"></param>
    /// <param name="userId"></param>
    /// <param name="fileName"></param>
    /// <param name="operation"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns>string: saved path on the blob</returns>
    private async Task<string> SaveImage(SKData sKData, string userId, string fileName, int operation, int width, int height, CancellationToken cancellationToken)
    {
        // Generate a unique file name for the image
        string uniqueFileName = Guid.NewGuid().ToString() + "_" + ChangeFileNameToWebp(fileName);

        // Combine path and uniqueFileName
        string blobPathName = Path.Combine(userId + "/" + operations[operation] + "/" +
                            Convert.ToString(width) + "x" + Convert.ToString(height) + "/" + uniqueFileName);

        // Get a reference to a blob
        BlobClient blobClient = _blobContainerClient.GetBlobClient(blobPathName);

        // Upload the file stream to the blob
        using (var fileStream = sKData.AsStream())
        {
            sKData.AsStream().Seek(0, SeekOrigin.Begin);

            await blobClient.UploadAsync(fileStream, overwrite: true, cancellationToken);
        }

        return blobPathName;
    }

    /// <summary>
    /// Save image on the disk with no inputs.
    /// The path is generated by combining: userId + operation + Guid generated uniqueFileName.
    /// Used in this class only. 
    /// </summary>
    /// <param name="sKData"></param>
    /// <param name="userId"></param>
    /// <param name="fileName"></param>
    /// <param name="operation"></param>
    /// <returns>string: saved path on the blob</returns>
    private async Task<string> SaveImage(SKData sKData, string userId, string fileName, int operation, CancellationToken cancellationToken)
    {
        // Generate a unique file name for the image
        string uniqueFileName = Guid.NewGuid().ToString() + "_" + ChangeFileNameToWebp(fileName);

        // Construct path and uniqueFileName
        string blobPathName = Path.Combine(userId + "/" + operations[operation] + "/" + uniqueFileName);

        // Get a reference to a blob
        BlobClient blobClient = _blobContainerClient.GetBlobClient(blobPathName);

        // Upload the file stream to the blob
        using (var fileStream = sKData.AsStream())
        {
            sKData.AsStream().Seek(0, SeekOrigin.Begin);

            await blobClient.UploadAsync(fileStream, overwrite: true, cancellationToken);
        }

        return blobPathName;
    }

    /// <summary>
    /// Saving the input image without changing it (as-is). 
    /// The path is generated by combining: userId + operation + Guid generated uniqueFileName.
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="userId"></param>
    /// <param name="operation"></param>
    /// <returns>string: saved path on the blob</returns>
    public async Task<string> SaveImageAsIs(IFormFile formFile, string userId, int operation, CancellationToken cancellationToken)
    {
        // Generate a unique file name for the image
        string uniqueFileName = Guid.NewGuid().ToString() + "_" + ChangeFileNameToWebp(formFile.FileName);

        // Construct path and uniqueFileName
        string blobPathName = Path.Combine(userId + "/" + operations[operation] + "/" + uniqueFileName);

        // Get a reference to a blob
        BlobClient blobClient = _blobContainerClient.GetBlobClient(blobPathName);

        // Upload the file stream to the blob
        await blobClient.UploadAsync(formFile.OpenReadStream(), overwrite: true, cancellationToken);

        return blobPathName;
    }
    #endregion SaveMethods

    #region Helpers
    /// <summary>
    /// Get formFile.fileName and convert any extensions to webp. e.g. my-photo.jpeg => my-photo.webp
    /// </summary>
    /// <param name="fileNameInput"></param>
    /// <returns>my-photo.webp</returns>
    private static string? ChangeFileNameToWebp(string fileNameInput)
    {
        return fileNameInput.Split(".")[0] + ".webp";
    }
    #endregion Helpers
}