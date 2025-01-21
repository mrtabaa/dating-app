namespace api.DTOs.Account;

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
    string Gender, // TODO: Convert it to Enum
    string RecaptchaToken
);