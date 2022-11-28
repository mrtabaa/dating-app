namespace api.DTOs;
public class LoginSuccessDto {
    public string? Token { get; set; }
    public bool Verified { get; set; }
    public string[]? PhotoUrls { get; set; }
    public string? ProfilePhotoUrl { get; set; }
}
