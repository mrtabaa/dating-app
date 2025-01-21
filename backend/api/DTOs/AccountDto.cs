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

public record LoginDto(
    [MaxLength(50)] string EmailUsername,
    [DataType(DataType.Password)]
    [Length(PropLength.PasswordMinLength, PropLength.PasswordMaxLength)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Needs: 8 to 50 characters. An uppercase character(ABC). A number(123)")]
    string Password,
    string RecaptchaToken
);

public record LoggedInDto(
    [Optional] bool IsRecaptchaTokenInvalid,
    [Optional] string? RecaptchaToken,
    [Optional] string? Email, // Used only to verify account. Will always return null if account is verified.
    [Optional] string? Token,
    [Optional] string? KnownAs,
    [Optional] string? UserName,
    [Optional] string? Gender,
    [Optional] string? ProfilePhotoUrl,
    [Optional] bool IsWrongCreds,
    [Optional] List<string> Errors,
    [Optional] bool IsProfileCompleted,
    [Optional] bool IsEmailNotConfirmed
);

// TODO: Implement it
public record OperationResult<T>(
    [Optional] bool IsSuccess,
    [Optional] T Result,
    [Optional] CustomError Error
);

public record CustomError(
    Enum Code,
    string? Message
);

public record ResetPasswordRequest(
    [MaxLength(PropLength.EmailManLength)]
    [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$", ErrorMessage = "Bad Email Format.")]
    string Email,
    string RecaptchaToken
);

public record ResetPassword(
    [Length(PropLength.UserNameMinLength, PropLength.UserNameMaxLength)]
    string Email,
    [DataType(DataType.Password)]
    [Length(PropLength.PasswordMinLength, PropLength.PasswordMaxLength)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Needs: 8 to 50 characters. An uppercase character(ABC). A number(123)")]
    string Password,
    [DataType(DataType.Password)]
    [Length(PropLength.PasswordMinLength, PropLength.PasswordMaxLength)]
    string ConfirmPassword,
    string ResetToken
);

public enum ErrorCode
{
    IsRecaptchaTokenInvalid,
    IsEmailAlreadyConfirmed,
    IsWrongCreds,
    NetIdentity,
    IsEmailNotConfirmed
}