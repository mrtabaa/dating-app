namespace api.DTOs;

public record UserRegisterDto(
    [
        MaxLength(50),
        RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$", ErrorMessage ="Bad Email Format.")
    ] string Email,
    [DataType(DataType.Password), Length(7, 20)] string Password,
    [DataType(DataType.Password), Length(7, 20)] string ConfirmPassword,
    [Length(2, 30)] string KnownAs,
    [Range(typeof(DateOnly), "1900-01-01", "2050-01-01")] DateOnly DateOfBirth,
    string Gender,
    [Length(10, 500)] string? Introduction,
    [Length(10, 500)] string? LookingFor,
    [Length(10, 500)] string? Interests,
    [Length(3, 30)] string City,
    [Length(3, 30)] string Country,
    List<Photo>? Photos
);

public record LoginDto(
    [
        RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$", ErrorMessage = "Bad Email Format."),
        MaxLength(50)
    ] string Email,
    [Length(7, 20)] string Password
);

public record LoggedInDto(
    string Token,
    string KnownAs,
    string? Email,
    string Gender,
    string? ProfilePhotoUrl,
    bool IsAlreadyExist,
    bool IsWrongCreds,
    bool IsFailed
);
