namespace api.DTOs.Account;

public record LoggedInDto(
    [Optional] bool IsRecaptchaTokenInvalid,
    [Optional] string? RecaptchaToken,
    [Optional] string? Email, // Used only to verify the account. Will always return null if the account is verified.
    [Optional] IEnumerable<string> RolesStr,
    [Optional] string? KnownAs,
    [Optional] string? UserName,
    [Optional] string? Gender,
    [Optional] string? ProfilePhotoUrl,
    [Optional] bool IsWrongCreds,
    [Optional] List<string> Errors,
    [Optional] bool IsProfileCompleted,
    [Optional] bool IsEmailNotConfirmed
);