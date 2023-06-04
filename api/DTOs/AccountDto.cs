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
    [MinLength(10), MaxLength(500)] string Introduction,
    [MinLength(10), MaxLength(500)] string LookingFor,
    [MinLength(10), MaxLength(500)] string Interests,
    [MinLength(3), MaxLength(30)] string City,
    [MinLength(3), MaxLength(30)] string Country,
    List<Photo> Photos
);

public record LoginDto(
    string? Schema,
    [
        RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$", ErrorMessage = "Bad Email Format."), 
        MaxLength(50)
    ] string Email,
    [MinLength(7), MaxLength(20)] string Password
);

public record LoginSuccessDto(
    string Schema,
    string? Token,
    string? Name,
    string? Email
);
