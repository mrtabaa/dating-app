namespace api.DTOs;

public record LoginDto(
    [EmailAddress, MaxLength(50)] string Email,
    [MinLength(7), MaxLength(20)] string Password
);

public record LoginSuccessDto(
    string? Token,
    string? Name,
    [EmailAddress] string? Email,
    bool BadEmailPattern
);

public record UserRegisterDto(
    [MinLength(2), MaxLength(20)] string Name,
    [EmailAddress, MaxLength(50)] string Email,
    [MinLength(7), MaxLength(20)] string Password
);
