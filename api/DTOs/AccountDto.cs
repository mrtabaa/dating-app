namespace api.DTOs;

public record UserRegisterDto(
    string? Schema,
    [MinLength(2), MaxLength(20)] string Name,
    [
        MaxLength(50),
        RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$", ErrorMessage ="Bad Email Format.")
    ] string Email,
    [DataType(DataType.Password), MinLength(7), MaxLength(20)] string Password,
    DateOnly DateOfBirth,
    string KnownAs,
    string Gender,
    string Introduction,
    string LookingFor,
    string Interests,
    string City,
    string Country,
    List<Photo> Photos
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
    [EmailAddress] string? Email
);
