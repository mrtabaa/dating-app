namespace api.DTOs;

public record RegisterDto(
    [
        MaxLength(50),
        RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$", ErrorMessage ="Bad Email Format.")
    ] string Email,
    [Length(1, 50)] string UserName,
    [DataType(DataType.Password), Length(8, 50), RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$", ErrorMessage ="Needs: 8 to 50 characters. An uppercase character(ABC). A number(123)")]
    string Password,
    [DataType(DataType.Password), Length(8, 50)] string ConfirmPassword,
    [Range(typeof(DateOnly), "1900-01-01", "2050-01-01")] DateOnly DateOfBirth,
    string Gender,
    string TurnstileToken
);

public record LoginDto(
    [MaxLength(50)] string EmailUsername,
    [DataType(DataType.Password), Length(8, 50), RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$", ErrorMessage ="Needs: 8 to 50 characters. An uppercase character(ABC). A number(123)")]
     string Password,
     string? TurnstileToken
);

public class LoggedInDto
{
    public bool IsTurnstileTokenInvalid { get; set; }
    public string? TurnstileToken { get; set; }
    public string? Token { get; init; }
    public string? KnownAs { get; init; }
    public string? UserName { get; init; }
    public string? Gender { get; init; }
    public string? ProfilePhotoUrl { get; init; }
    public bool IsWrongCreds { get; set; }
    public List<string> Errors { get; init; } = [];
    public bool IsProfileCompleted { get; init; }
}
