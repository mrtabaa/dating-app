namespace api.DTOs;
public class UserDto {
    public string? Id { get; init; }
    
    public string? Email { get; set; }
    
    public int Power { get; set; }
    
    public bool Verified { get; set; }
    
    public string? Name { get; set; }
    
    public string[]? PhotoUrls { get; set; }
    
    public string? ProfilePhotoUrl { get; set; }
    
    public string? Token { get; set; }
}
