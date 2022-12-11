namespace api.DTOs;

public record LoginDto(
    [EmailAddress, MaxLength(50)] string Email,
    [MinLength(7), MaxLength(20)] string Password
);

public record LoginSuccessDto(
    string? Token,
    [EmailAddress] string? Email,
    bool BadEmailPattern
);

public record UserRegisterDto(
    [EmailAddress, MaxLength(50)] string Email,
    [MinLength(7), MaxLength(20)] string Password
);
