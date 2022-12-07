namespace api.DTOs;
public class LoginDto {
    [EmailAddress, MaxLength(50)]
    public required string Email { get; set; }

    [MinLength(7), MaxLength(20)]
    public required string Password { get; set; }
}
