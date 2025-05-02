namespace Da.Application.DTOs.Account;

public record ResetPassword(
    [Length(PropLength.UserNameMinLength, PropLength.UserNameMaxLength)]
    string Email,
    [DataType(DataType.Password)]
    [Length(PropLength.PasswordMinLength, PropLength.PasswordMaxLength)]
    [RegularExpression(
        @"^(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Needs: 8 to 50 characters. An uppercase character(ABC). A number(123)"
    )]
    string Password,
    [DataType(DataType.Password)]
    [Length(PropLength.PasswordMinLength, PropLength.PasswordMaxLength)]
    string ConfirmPassword,
    string ResetToken
);