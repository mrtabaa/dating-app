namespace api.DTOs;

public record RegisterDto(
    [
        MaxLength(50),
        RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$", ErrorMessage ="Bad Email Format.")
    ] string Email,
    [Length(1, 50)] string UserName,
    [DataType(DataType.Password), Length(8, 50), RegularExpression(@"^(?=.*[A-Z])(?=.*[^\w\s]).*$")]
    string Password,
    [DataType(DataType.Password), Length(8, 50)] string ConfirmPassword,
    [Range(typeof(DateOnly), "1900-01-01", "2050-01-01")] DateOnly DateOfBirth,
    string Gender
);

public record LoginDto(
    [MaxLength(50)] string EmailUsername,
    [DataType(DataType.Password), Length(8, 50), RegularExpression(@"^(?=.*[A-Z])(?=.*[^\w\s]).*$")] // Uppercase + Special char + 8 chars
     string Password
);

public class LoggedInDto
{
    public string? Token { get; set; }
    public string? KnownAs { get; set; }
    public string? UserName { get; set; }
    public string? Gender { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public bool IsWrongCreds { get; set; }
    public List<string> Errors { get; set; } = [];
}
