namespace api.DTOs;
public class LoginDto {
    [EmailAddress]
    public required string Email { get; set; }

    [MinLength(7)]
    public required string Password { get; set; }
}
