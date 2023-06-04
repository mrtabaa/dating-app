using System.Reflection.Metadata;
using SkiaSharp;

namespace api.Extensions;

public static class ResizePhotoExtensions
{
    #region Variables
    private static string[] sizesString = { "128x128", "256x256", "512x512", "1024x1024", "scale" };
    private static int[] sizesInt = { 128, 256, 512, 1024 };

    enum Size
    {
        small_128,
        meduim_256,
        large_512,
        x_large_1028,
        scale
    }
    #endregion Variables

    #region Resize By Scale
    public static async Task ResizeCreateImageByScale(string folderPath, string filePath, string fileName, long fileLength)
    {
        float resizeFactor = 0.5f;

        switch (fileLength)
        {
            case > 3_000_000:
                resizeFactor = 0.3f;
                break;
            case > 2_000_000:
                resizeFactor = 0.5f;
                break;
            case > 1_000_000:
                resizeFactor = 0.7f;
                break;
            case > 500_000:
                resizeFactor = 0.9f;
                break;
        }

        SKBitmap bitmap = SKBitmap.Decode(filePath);

        int width = (int)Math.Round(bitmap.Width * resizeFactor);
        int height = (int)Math.Round(bitmap.Height * resizeFactor);

        SKBitmap bitmap2 = new(width, height, bitmap.ColorType, bitmap.AlphaType);

        SKCanvas canvas = new(bitmap2);

        canvas.SetMatrix(SKMatrix.CreateScale(resizeFactor, resizeFactor));
        canvas.DrawBitmap(bitmap, new SKPoint());
        canvas.ResetMatrix();
        canvas.Flush();

        SKImage sKImage = SKImage.FromBitmap(bitmap2);
        SKData sKData = sKImage.Encode(SKEncodedImageFormat.Jpeg, 100);

        await SaveImage(sKData, folderPath, fileName, sizesString[(int)Size.scale]);
    }
    #endregion Resize By Scale

    #region Resize by Fixed Size
    // public static async Task ResizeCreateImage_128x128(string folderPath, string filePath, string fileName)
    // {
    //     using SKBitmap sourceBitmap = SKBitmap.Decode(filePath);

    //     int width = Math.Min(sizesInt[(int)Size.small_128], sourceBitmap.Width);
    //     int height = Math.Min(sizesInt[(int)Size.small_128], sourceBitmap.Height);

    //     using SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.High);

    //     using SKImage scaledImage = SKImage.FromBitmap(scaledBitmap);
    //     using SKData sKData = scaledImage.Encode(SKEncodedImageFormat.Jpeg, 100);

    //     await SaveImage(sKData, folderPath, fileName, sizesString[(int)Size.small_128]);
    // }
    public static async Task ResizeCreateImage_128x128(string folderPath, string filePath, string fileName)
    {
        using (SKBitmap sourceBitmap = SKBitmap.Decode(filePath))
        {
            if (sourceBitmap.Width < sourceBitmap.Height)
            {
                var rotated = new SKBitmap(sourceBitmap.Height, sourceBitmap.Width);

                using (var surface = new SKCanvas(rotated))
                {
                    surface.Translate(rotated.Width, 0);
                    surface.RotateDegrees(90);
                    surface.DrawBitmap(sourceBitmap, 0, 0);
                }

                using SKImage rotatedScaledImage = SKImage.FromBitmap(rotated);
                using SKData rotatedSKData = rotatedScaledImage.Encode(SKEncodedImageFormat.Jpeg, 100);

                await SaveImage(rotatedSKData, folderPath, fileName, sizesString[(int)Size.small_128]);
            }

            int width = Math.Min(sizesInt[(int)Size.small_128], sourceBitmap.Width);
            int height = Math.Min(sizesInt[(int)Size.small_128], sourceBitmap.Height);

            using SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.High);

            using SKImage scaledImage = SKImage.FromBitmap(sourceBitmap);
            using SKData sKData = scaledImage.Encode(SKEncodedImageFormat.Jpeg, 100);

            await SaveImage(sKData, folderPath, fileName, sizesString[(int)Size.small_128]);
        }
    }

    public static SKBitmap Rotate(string filePath)
    {
        using (var bitmap = SKBitmap.Decode(filePath))
        {
            var rotated = new SKBitmap(bitmap.Height, bitmap.Width);

            using (var surface = new SKCanvas(rotated))
            {
                surface.Translate(rotated.Width, 0);
                surface.RotateDegrees(90);
                surface.DrawBitmap(bitmap, 0, 0);
            }

            return rotated;
        }
    }

    public static async Task ResizeCreateImage_256x256(string folderPath, string filePath, string fileName)
    {
        using SKBitmap sourceBitmap = SKBitmap.Decode(filePath);

        int height = Math.Min(sizesInt[(int)Size.meduim_256], sourceBitmap.Height);
        int width = Math.Min(sizesInt[(int)Size.meduim_256], sourceBitmap.Width);

        using SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
        using SKImage scaledImage = SKImage.FromBitmap(scaledBitmap);
        using SKData sKData = scaledImage.Encode(SKEncodedImageFormat.Jpeg, 100);

        await SaveImage(sKData, folderPath, fileName, sizesString[(int)Size.meduim_256]);
    }

    public static async Task ResizeCreateImage_512x512(string folderPath, string filePath, string fileName)
    {
        using SKBitmap sourceBitmap = SKBitmap.Decode(filePath);

        int height = Math.Min(sizesInt[(int)Size.large_512], sourceBitmap.Height);
        int width = Math.Min(sizesInt[(int)Size.large_512], sourceBitmap.Width);

        using SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
        using SKImage scaledImage = SKImage.FromBitmap(scaledBitmap);
        using SKData sKData = scaledImage.Encode(SKEncodedImageFormat.Jpeg, 100);

        await SaveImage(sKData, folderPath, fileName, sizesString[(int)Size.large_512]);
    }

    public static async Task ResizeCreateImage_1024x1024(string folderPath, string filePath, string fileName)
    {
        using SKBitmap sourceBitmap = SKBitmap.Decode(filePath);

        int height = Math.Min(sizesInt[(int)Size.x_large_1028], sourceBitmap.Height);
        int width = Math.Min(sizesInt[(int)Size.x_large_1028], sourceBitmap.Width);

        using SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
        using SKImage scaledImage = SKImage.FromBitmap(scaledBitmap);
        using SKData sKData = scaledImage.Encode(SKEncodedImageFormat.Jpeg, 100);

        await SaveImage(sKData, folderPath, fileName, sizesString[(int)Size.x_large_1028]);
    }

    #endregion Resize by Fixed Size

    #region Save Method
    private static async Task SaveImage(SKData sKData, string inputFolderPath, string fileName, string size)
    {
        string folderPathBySize = Path.Combine(inputFolderPath, size);
        if (!Directory.Exists(folderPathBySize)) // create folder
        {
            Directory.CreateDirectory(folderPathBySize);
        }

        string filePath = Path.Combine(folderPathBySize, fileName);

        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        {
            sKData.AsStream().Seek(0, SeekOrigin.Begin);
            await sKData.AsStream().CopyToAsync(fileStream);

            fileStream.Flush();
            fileStream.Close();
        }
    }
    #endregion Save Method
}
