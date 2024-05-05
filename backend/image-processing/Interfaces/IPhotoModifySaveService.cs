namespace image_processing.Interfaces;

public interface IPhotoModifySaveService
{
    public Task<string?> ResizeImageByScale(IFormFile formFile, string userId, int standardSizeIndex, CancellationToken cancellationToken);
    public Task<string?> ResizeByPixel(IFormFile formFile, string userId, int widthIn, int heightIn, CancellationToken cancellationToken);
    public Task<string?> ResizeByPixel_Square(IFormFile formFile, string userId, int side, CancellationToken cancellationToken);
    public Task<string?> Crop(IFormFile formFile, string userId, int widthIn, int heightIn, CancellationToken cancellationToken);
    public Task<string?> Crop_Square(IFormFile formFile, string userId, int side, CancellationToken cancellationToken);
    public Task<string?> CropWithOriginalSide_Square(IFormFile formFile, string userId, CancellationToken cancellationToken);
    public Task<string?> SaveImageAsIs(IFormFile formFile, string userId, int operation, CancellationToken cancellationToken);
}