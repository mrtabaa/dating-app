using SkiaSharp;

namespace api.Extensions
{
    public interface IPhotoModifyService
    {
        public Task<string?> ResizeImageByScale(IFormFile formFile, string userId);
        public Task<string?> ResizeImageByPixel(IFormFile formFile, string userId, int widthIn, int heightIn);
        public Task<string?> CropImageAndSave(IFormFile formFile, string userId, int widthIn, int heightIn);
    }
}