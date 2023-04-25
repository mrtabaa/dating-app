namespace api.DTOs;

public record UserRegisterDto(
    string? Schema,
    [MinLength(2), MaxLength(20)] string Name,
    [EmailAddress, MaxLength(50)] string Email,
    [MinLength(7), MaxLength(20)] string Password,
    DateOnly DateOfBirth,
    string KnownAs,
    string Gender,
    string Introduction,
    string LookingFor,
    string Interests,
    string City,
    string Country,
    IEnumerable<Photo> Photos
);

public record LoginDto(
    string? Schema,
    [EmailAddress, MaxLength(50)] string Email,
    [MinLength(7), MaxLength(20)] string Password
);

public record LoginSuccessDto(
    string Schema,
    string? Token,
    string? Name,
    [EmailAddress] string? Email,
    bool BadEmailPattern
);
