namespace api.Models.Helpers;

public class PhotoUploadStatus
{
    public const int MaxPhotosLimit = 100;
    public bool IsMaxPhotoReached { get; set; }
    public Photo? Photo { get; set; }
}