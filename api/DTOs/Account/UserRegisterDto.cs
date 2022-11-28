namespace api.DTOs;
public class UserRegisterDto {

    [EmailAddress]
    public required string Email { get; set; }

    [MinLength(7)]
    public required string Password { get; set; }

    public required string Name { get; set; }

    public string[]? PhotoUrls { get; set; }

    public string? ProfilePhotoUrl { get; set; }

    [MinLength(1), MaxLength(3)]
    public required UniversityStringDto[]? Universities { get; set; }
}
