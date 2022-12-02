namespace api.DTOs;
public class UserRegisterDto {

    [EmailAddress]
    public required string Email { get; set; }

    [MinLength(7)]
    public required string Password { get; set; }
}
