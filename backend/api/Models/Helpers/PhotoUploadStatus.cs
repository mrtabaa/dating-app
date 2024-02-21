namespace api.Models.Helpers;

public class PhotoUploadStatus
{
    public bool IsMaxReached { get; set; }
    public Photo? Photo { get; set; }
}
