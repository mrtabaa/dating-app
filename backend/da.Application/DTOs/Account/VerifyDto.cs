namespace da.Application.DTOs.Account;

public record VerifyDto(
    [MaxLength(PropLength.EmailManLength)]
    [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$", ErrorMessage = "Bad Email Format.")]
    string Email,
    [Length(6, 6)]
    [RegularExpression(@"^\d+$", ErrorMessage = "Only digits accepted.")]
    string Code
);