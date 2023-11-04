using SkiaSharp;

namespace api.Services;

public class PhotoModifySaveService : IPhotoModifySaveService
{
    #region Constructor and vars
    private readonly IWebHostEnvironment _webHostEnvironment;
    const string storageAddress = "storage/photos/";

    readonly string[] operations = { "resize-scale", "resize-pixel", "resize-pixel-square", "crop", "original" };
    private enum OperationName
    {
        ResizeByScale,
        ResizeByPixel,
        ResizeByPixelSquare,
        Crop,
        Original
    }

    public PhotoModifySaveService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }
    #endregion

    #region Resize Methods

    /// <summary>
    /// Reduce by Scaling to around 300k
    /// Skip if already less than 300k
    /// Save image to hard if successful.
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="userId"></param>
    /// <returns>filePath</returns>
    public async Task<string?> ResizeImageByScale(IFormFile formFile, string userId)
    {
        // performace
        if (formFile.Length < 300_000)
            return await SaveImageAsIs(formFile, userId, (int)OperationName.Original); // return filePath

        // do the job
        float resizeFactor;
        switch (formFile.Length)
        {
            case < 500_000:
                resizeFactor = 0.9f;
                break;
            case < 1_000_000:
                resizeFactor = 0.72f;
                break;
            case < 2_000_000:
                resizeFactor = 0.32f;
                break;
            case < 3_000_000:
                resizeFactor = 0.3f;
                break;
            case < 4_000_000:
                resizeFactor = 0.2f;
                break;
            default:
                resizeFactor = 0.15f;
                break;
        }

        using (var binaryReader = new BinaryReader(formFile.OpenReadStream()))
        {
            // get image from formFile
            byte[]? imageData = binaryReader.ReadBytes((int)formFile.Length);

            // convert imageData to SKImage
            using SKImage skImageSource = SKImage.FromEncodedData(imageData);
            using SKBitmap bitmapSource = SKBitmap.FromImage(skImageSource);

            int width = (int)Math.Round(bitmapSource.Width * resizeFactor);
            int height = (int)Math.Round(bitmapSource.Height * resizeFactor);

            using SKBitmap bitmapResized = new(width, height, bitmapSource.ColorType, bitmapSource.AlphaType);

            using SKCanvas canvas = new(bitmapResized);

            canvas.SetMatrix(SKMatrix.CreateScale(resizeFactor, resizeFactor));
            canvas.DrawBitmap(bitmapSource, new SKPoint());
            canvas.ResetMatrix();
            canvas.Flush();

            using SKImage sKImageResized = SKImage.FromBitmap(bitmapResized);
            using SKData sKData = sKImageResized.Encode(SKEncodedImageFormat.Webp, 100);

            string? filePath = await SaveImage(sKData, userId, formFile.FileName, (int)OperationName.ResizeByScale);

            return filePath;
        }
    }

    /// <summary>
    /// Resize image based on the input sides.
    /// It does NOT keep the original image's scale if proportional input sides are not give. 
    /// SaveImageAsIs if either input side is larger than the actual image's coresponding side.
    /// Save image to hard if successful.
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="userId"></param>
    /// <param name="widthIn"></param>
    /// <param name="heightIn"></param>
    /// <returns></returns>
    public async Task<string?> ResizeByPixel(IFormFile formFile, string userId, int widthIn, int heightIn)
    {
        // do the job
        using (var binaryReader = new BinaryReader(formFile.OpenReadStream()))
        {
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
                return await SaveImageAsIs(formFile, userId, (int)OperationName.Original);
            #endregion

            #region images with sides greater than input sides
            using SKBitmap sourceBitmap = SKBitmap.FromImage(skImage);

            // resize
            using SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(widthIn, heightIn), SKFilterQuality.High);

            using SKImage scaledImage = SKImage.FromBitmap(scaledBitmap);
            using SKData sKData = scaledImage.Encode(SKEncodedImageFormat.Webp, 100);

            string? filePath = await SaveImage(sKData, userId, formFile.FileName, (int)OperationName.ResizeByPixel, widthIn, heightIn);

            return filePath;
            #endregion
        }
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
    public async Task<string?> ResizeByPixel_Square(IFormFile formFile, string userId, int sideIn)
    {
        using (var binaryReader = new BinaryReader(formFile.OpenReadStream()))
        {
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
                return await SaveImageAsIs(formFile, userId, (int)OperationName.Original); // return filePath
            #endregion

            #region 3 x 5 original image sides 
            // 6 in => 3 x 3
            // 3 in => 3 x 3
            // sideIn >= original min side (e.g sideIn = 6)
            if (sideIn >= Math.Min(skImage.Width, skImage.Height))
                return await CropWithOriginalSide_Square(formFile, userId);
            #endregion

            #region 3 x 5 original image sides 
            // 2 in => 2 x 2
            // sideIn < original min side of 3 (e.g sideIn = 2)
            // crop to square (3 x 3) first, then resize by 2
            int smallerSide = Math.Min(skImage.Width, skImage.Height);

            SKImage? croppedImage = CropImageForResize(skImage, smallerSide, smallerSide);

            // resize
            if (croppedImage is not null)
            {
                using SKBitmap croppedBitmap = SKBitmap.FromImage(croppedImage);
                using SKBitmap scaledBitmap = croppedBitmap.Resize(new SKImageInfo(sideIn, sideIn), SKFilterQuality.High);

                using SKImage scaledImage = SKImage.FromBitmap(scaledBitmap);
                using SKData sKData = scaledImage.Encode(SKEncodedImageFormat.Webp, 100);

                return await SaveImage(sKData, userId, formFile.FileName, (int)OperationName.ResizeByPixelSquare, sideIn, sideIn);
            }
            #endregion

            return null;
        }
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
    public async Task<string?> Crop(IFormFile formFile, string userId, int widthIn, int heightIn)
    {
        using (var binaryReader = new BinaryReader(formFile.OpenReadStream()))
        {
            // get image from formFile
            byte[]? imageData = binaryReader.ReadBytes((int)formFile.Length);

            // convert imageData to SKImage
            using SKImage skImage = SKImage.FromEncodedData(imageData);

            #region Inputs are larger than original sides
            /// 3 x 5 or 3 x 3 image original sides
            // 4 in 6 in => 3 x 5
            // 3 in 5 in => 3 x 5
            if (widthIn <= skImage.Width && heightIn <= skImage.Height)
                return await SaveImageAsIs(formFile, userId, (int)OperationName.Original);
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
            using SKData sKData = croppedImage.Encode(SKEncodedImageFormat.Webp, 100);

            return await SaveImage(sKData, userId, formFile.FileName, (int)OperationName.Crop, widthIn, heightIn);
            #endregion
        }
    }

    /// <summary>
    /// 
    /// Save image to hard if successful.
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="userId"></param>
    /// <param name="sideIn"></param>
    /// <returns></returns>
    public async Task<string?> Crop_Square(IFormFile formFile, string userId, int sideIn)
    {
        using (var binaryReader = new BinaryReader(formFile.OpenReadStream()))
        {
            // get image from formFile
            byte[]? imageData = binaryReader.ReadBytes((int)formFile.Length);

            // convert imageData to SKImage
            using SKImage skImage = SKImage.FromEncodedData(imageData);

            // check if the given side is not larger than the image size
            if (sideIn * sideIn > formFile.Length && skImage.Width == skImage.Height)
                return await SaveImageAsIs(formFile, userId, (int)OperationName.Original);
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
                using SKData sKData = croppedImage.Encode(SKEncodedImageFormat.Webp, 100);

                return await SaveImage(sKData, userId, formFile.FileName, (int)OperationName.Crop, sideIn, sideIn);
            }
        }
    }

    /// <summary>
    /// 
    /// Save image to hard if successful.
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<string?> CropWithOriginalSide_Square(IFormFile formFile, string userId)
    {
        using (var binaryReader = new BinaryReader(formFile.OpenReadStream()))
        {
            // get image from formFile
            byte[]? imageData = binaryReader.ReadBytes((int)formFile.Length);

            // convert imageData to SKImage
            using SKImage skImage = SKImage.FromEncodedData(imageData);

            //performance: Skip, already square.
            if (skImage.Width == skImage.Height)
                return await SaveImageAsIs(formFile, userId, (int)OperationName.Original);


            int side = Math.Min(skImage.Width, skImage.Height);

            // find the center
            int centerX = skImage.Width / 2;
            int centerY = skImage.Height / 2;

            // find the start points
            int startX = centerX - side / 2;
            int startY = centerY - side / 2;

            // crop image
            SKImage croppedImage = skImage.Subset(SKRectI.Create(startX, startY, side, side));
            using SKData sKData = croppedImage.Encode(SKEncodedImageFormat.Webp, 100);

            return await SaveImage(sKData, userId, formFile.FileName, (int)OperationName.Crop, side, side);
        }
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
    private async Task<string?> SaveImage(SKData sKData, string userId, string fileName, int operation, int width, int height)
    {
        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, storageAddress, userId, operations[operation],
                            Convert.ToString(width) + "x" + Convert.ToString(height));

        // find path OR create folder if doesn't exist by userId
        if (!Directory.Exists(uploadsFolder)) // create folder
            Directory.CreateDirectory(uploadsFolder);

        string uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;

        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        {
            sKData.AsStream().Seek(0, SeekOrigin.Begin);
            await sKData.AsStream().CopyToAsync(fileStream);

            fileStream.Flush();
            fileStream.Close();
        }

        return filePath;
    }

    private async Task<string?> SaveImage(SKData sKData, string userId, string fileName, int operation)
    {
        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, storageAddress, userId, operations[operation]);

        // find path OR create folder if doesn't exist by userId
        if (!Directory.Exists(uploadsFolder)) // create folder
            Directory.CreateDirectory(uploadsFolder);

        string uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;

        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        {
            sKData.AsStream().Seek(0, SeekOrigin.Begin);
            await sKData.AsStream().CopyToAsync(fileStream);

            fileStream.Flush();
            fileStream.Close();
        }

        return filePath;
    }

    public async Task<string?> SaveImageAsIs(IFormFile formFile, string userId, int operation)
    {
        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, storageAddress, userId, operations[operation]);

        // find path OR create folder if doesn't exist by userId
        if (!Directory.Exists(uploadsFolder)) // create folder
            Directory.CreateDirectory(uploadsFolder);

        string uniqueFileName = Guid.NewGuid().ToString() + "_" + formFile.FileName;

        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        Path.Combine(uploadsFolder + uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await formFile.CopyToAsync(stream);
        }

        return filePath;
    }
    #endregion SaveMethods
}