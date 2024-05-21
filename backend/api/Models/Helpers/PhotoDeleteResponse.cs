namespace api.Models.Errors;

public class PhotoDeleteResponse
{
    public string? NewMainUrl { get; set; } // It's set only when the main photo is deleted. It will have the next main photo's blobUrl
    public string? SuccessMessage { get; set; }
    public bool IsDeletionFailed { get; set; }
}
