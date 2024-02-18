namespace api.DTOs;

public record UserRegisterDto(
    [
        MaxLength(50),
        RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$", ErrorMessage ="Bad Email Format.")
    ] string Email,
    [Length(3, 50)] string UserName,
    [DataType(DataType.Password), Length(7, 20)] string Password,
    [DataType(DataType.Password), Length(7, 20)] string ConfirmPassword,
    [Length(2, 30)] string KnownAs,
    [Range(typeof(DateOnly), "1900-01-01", "2050-01-01")] DateOnly DateOfBirth,
    string Gender,
    [Length(10, 500)] string? Introduction,
    [Length(10, 500)] string? LookingFor,
    [Length(10, 500)] string? Interests,
    [Length(3, 30)] string City,
    [Length(3, 30)] string Country,
    List<Photo>? Photos
);

public record LoginDto(
    [
        RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$", ErrorMessage = "Bad Email Format."),
        MaxLength(50)
    ] string UserName,
    [Length(7, 20)] string Password
);

public class LoggedInDto
{
    public string? Token { get; set; }
    public string? KnownAs { get; set; }
    public string? UserName { get; set; }
    public string? Gender { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public bool IsAlreadyExist { get; set; }
    public bool IsWrongCreds { get; set; }
    public bool IsFailed { get; set; }
}
