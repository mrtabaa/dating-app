namespace api.Interfaces;

public interface IPhotoModifyService
{
    public Task<string?> ResizeImageByScale(IFormFile formFile, string userId);
    public Task<string?> ResizeByPixel(IFormFile formFile, string userId, int widthIn, int heightIn);
    public Task<string?> ResizeByPixel_Square(IFormFile formFile, string userId, int side);
    public Task<string?> CropAndSave(IFormFile formFile, string userId, int widthIn, int heightIn);
}