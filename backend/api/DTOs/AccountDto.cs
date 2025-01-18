namespace api.DTOs;

internal static class PropLength
{
    internal const int EmailManLength = 100;
    internal const int UserNameMinLength = 1;
    internal const int UserNameMaxLength = 50;
    internal const int PasswordMinLength = 8;
    internal const int PasswordMaxLength = 50;
}

public record RegisterDto(
    [MaxLength(PropLength.EmailManLength)]
    [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$", ErrorMessage = "Bad Email Format.")]
    string Email,
    [Length(PropLength.UserNameMinLength, PropLength.UserNameMaxLength)]
    string UserName,
    [DataType(DataType.Password)]
    [Length(PropLength.PasswordMinLength, PropLength.PasswordMaxLength)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Needs: 8 to 50 characters. An uppercase character(ABC). A number(123)")]
    string Password,
    [DataType(DataType.Password)]
    [Length(PropLength.PasswordMinLength, PropLength.PasswordMaxLength)]
    string ConfirmPassword,
    [Range(typeof(DateOnly), "1900-01-01", "2050-01-01")]
    DateOnly DateOfBirth,
    string Gender,
    string RecaptchaToken
);

public record RegisteredDto(
    [Optional] bool IsSuccess,
    [Optional] bool IsRecaptchaTokenInvalid,
    [Optional] string? ErrorMessage
);

public record VerifyDto(
    [MaxLength(PropLength.EmailManLength)]
    [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$", ErrorMessage = "Bad Email Format.")]
    string Email,
    [Length(6, 6)]
    [RegularExpression(@"^\d+$", ErrorMessage = "Only digits accepted.")]
    string Code
);

public record ResendCodeRequest(
    [MaxLength(PropLength.EmailManLength)]
    [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$", ErrorMessage = "Bad Email Format.")]
    string Email,
    string RecaptchaToken
);

public record ResendCodeResult(
    [Optional] bool IsRecaptchaTokenInvalid,
    [Optional] bool IsSuccessful
);

public record LoginDto(
    [MaxLength(50)] string EmailUsername,
    [DataType(DataType.Password)]
    [Length(PropLength.PasswordMinLength, PropLength.PasswordMaxLength)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Needs: 8 to 50 characters. An uppercase character(ABC). A number(123)")]
    string Password,
    string RecaptchaToken
);

public class LoggedInDto
{
    public bool IsRecaptchaTokenInvalid { get; set; }
    public string? RecaptchaToken { get; set; }
    public string? Email { get; set; } // Used only to verify account. Will always return null if account is verified.
    public string? Token { get; init; }
    public string? KnownAs { get; init; }
    public string? UserName { get; set; }
    public string? Gender { get; init; }
    public string? ProfilePhotoUrl { get; init; }
    public bool IsWrongCreds { get; set; }
    public List<string> Errors { get; init; } = [];
    public bool IsProfileCompleted { get; init; }
    public bool IsEmailNotConfirmed { get; set; }
}