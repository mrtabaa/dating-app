namespace api.DTOs.Account;

public record ResetPasswordRequest(
    [MaxLength(PropLength.EmailManLength)]
    [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$", ErrorMessage = "Bad Email Format.")]
    string Email,
    string RecaptchaToken
);