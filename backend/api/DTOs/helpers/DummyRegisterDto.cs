namespace api.DTOs;

public record DummyRegisterDto(
    [
        MaxLength(50),
        RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$", ErrorMessage ="Bad Email Format.")
    ] string Email,
    [Length(1, 50)] string UserName,
    [DataType(DataType.Password), Length(8, 20), RegularExpression(@"^(?=.*[A-Z])(?=.*[^\w\s]).*$")]
    string Password,
    [DataType(DataType.Password), Length(8, 20)] string ConfirmPassword,
    [Length(1, 50)] string KnownAs,
    [Range(typeof(DateOnly), "1900-01-01", "2050-01-01")] DateOnly DateOfBirth,
    string Gender,
    [Length(3, 30)] string City,
    [Length(3, 30)] string Country,
    [MaxLength(1000)] string? Introduction,
    [MaxLength(1000)] string? LookingFor,
    [MaxLength(1000)] string? Interests
);

