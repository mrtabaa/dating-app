namespace api.DTOs.Account;

public record LoginDto(
    [MaxLength(50)] string EmailUsername,
    [DataType(DataType.Password)]
    [Length(PropLength.PasswordMinLength, PropLength.PasswordMaxLength)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Needs: 8 to 50 characters. An uppercase character(ABC). A number(123)")]
    string Password,
    string RecaptchaToken
);