using SkiaSharp;

namespace api.Services;

public class PhotoModifySaveService : IPhotoModifySaveService
{
    #region Constructor and vars
    private readonly IWebHostEnvironment _webHostEnvironment;

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

    public async Task<string?> ResizeImageByScale(IFormFile formFile, string userId)
    {
        // performace
        if (formFile.Length < 300_000)
            return await SaveImage(formFile, userId, formFile.Name, (int)OperationName.Original); // return filePath

        // do the job
        float resizeFactor = 0;

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
            using SKData sKData = sKImageResized.Encode(SKEncodedImageFormat.Jpeg, 100);

            string? filePath = await SaveImage(sKData, userId, formFile.FileName, (int)OperationName.ResizeByScale);

            return filePath;
        }
    }

    public async Task<string?> ResizeByPixel(IFormFile formFile, string userId, int widthIn, int heightIn)
    {
        // performace
        if (widthIn * heightIn <= formFile.Length)
            return await SaveImage(formFile, userId, formFile.Name, (int)OperationName.Original); // return filePath

        // do the job
        using (var binaryReader = new BinaryReader(formFile.OpenReadStream()))
        {
            // get image from formFile
            byte[]? imageData = binaryReader.ReadBytes((int)formFile.Length);

            // convert imageData to SKImage
            using SKImage skImage = SKImage.FromEncodedData(imageData);

            using SKBitmap sourceBitmap = SKBitmap.FromImage(skImage);

            int width = Math.Min(widthIn, sourceBitmap.Width);
            int height = Math.Min(heightIn, sourceBitmap.Height);

            // resize
            using SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.High);

            using SKImage scaledImage = SKImage.FromBitmap(scaledBitmap);
            using SKData sKData = scaledImage.Encode(SKEncodedImageFormat.Jpeg, 100);

            string? filePath = await SaveImage(sKData, userId, formFile.FileName, (int)OperationName.ResizeByPixel, widthIn, heightIn);

            return filePath;
        }
    }

    public async Task<string?> ResizeByPixel_Square(IFormFile formFile, string userId, int sideIn)
    {
        // performace
        using (var binaryReader = new BinaryReader(formFile.OpenReadStream()))
        {
            // get image from formFile
            byte[]? imageData = binaryReader.ReadBytes((int)formFile.Length);

            // convert imageData to SKImage
            using SKImage skImage = SKImage.FromEncodedData(imageData);

            ///// performance: skip resize
            if (sideIn * sideIn <= formFile.Length && skImage.Width == skImage.Height)
                // save original file
                return await SaveImage(formFile, userId, formFile.Name, (int)OperationName.Original); // return filePath

            if (sideIn * sideIn <= formFile.Length)
                // crop to square and save
                return await CropAndSave(formFile, userId, sideIn, sideIn);
            /////

            // get the smaller side to crop with square shape
            int equalSide = Math.Min(skImage.Width, skImage.Height);

            // crop
            SKImage? croppedImage = CropImageForResize(skImage, equalSide, equalSide);

            if (croppedImage is not null)
            {
                // resize
                using SKBitmap croppedBitmap = SKBitmap.FromImage(croppedImage);
                using SKBitmap scaledBitmap = croppedBitmap.Resize(new SKImageInfo(sideIn, sideIn), SKFilterQuality.High);

                using SKImage scaledImage = SKImage.FromBitmap(scaledBitmap);
                using SKData sKData = scaledImage.Encode(SKEncodedImageFormat.Jpeg, 100);

                string? filePath = await SaveImage(sKData, userId, formFile.FileName, (int)OperationName.ResizeByPixelSquare, sideIn, sideIn);

                return filePath;
            }

            return null;
        }
    }

    #endregion Resize Methods 

    #region Crop Methods
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

    public async Task<string?> CropAndSave(IFormFile formFile, string userId, int widthIn, int heightIn)
    {
        using (var binaryReader = new BinaryReader(formFile.OpenReadStream()))
        {
            // get image from formFile
            byte[]? imageData = binaryReader.ReadBytes((int)formFile.Length);

            // convert imageData to SKImage
            using SKImage skImage = SKImage.FromEncodedData(imageData);

            // check if the given sides are not larger than the image size
            if (widthIn < skImage.Width && heightIn < skImage.Height)
            {
                // find the center
                int centerX = skImage.Width / 2;
                int centerY = skImage.Height / 2;

                // find the start points
                int startX = centerX - widthIn / 2;
                int startY = centerY - heightIn / 2;

                // crop image
                SKImage croppedImage = skImage.Subset(SKRectI.Create(startX, startY, widthIn, heightIn));
                using SKData sKData = croppedImage.Encode(SKEncodedImageFormat.Jpeg, 100);

                string? filePath = await SaveImage(sKData, userId, formFile.FileName, (int)OperationName.Crop, widthIn, heightIn);

                return filePath;
            }
            else return null;
        }
    }
    #endregion Crop Methods

    #region Save Methods
    private async Task<string?> SaveImage(SKData sKData, string userId, string fileName, int operation, int width, int height)
    {
        string uploadsFolder = string.Empty;

        uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Storage/Photos/", userId, operations[operation],
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
        string uploadsFolder = string.Empty;

        uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Storage/Photos/", userId, operations[operation]);

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

    private async Task<string?> SaveImage(IFormFile formFile, string userId, string fileName, int operation)
    {
        string uploadsFolder = string.Empty;

        uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Storage/Photos/", userId, operations[operation]);

        // find path OR create folder if doesn't exist by userId
        if (!Directory.Exists(uploadsFolder)) // create folder
            Directory.CreateDirectory(uploadsFolder);

        string uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;

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