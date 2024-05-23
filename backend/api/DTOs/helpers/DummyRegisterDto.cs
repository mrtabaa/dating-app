namespace api.DTOs;

public record DummyRegisterDto(
    [
        MaxLength(50),
        RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$", ErrorMessage ="Bad Email Format.")
    ] string Email,
    [Length(1, 50)] string UserName,
    [DataType(DataType.Password), Length(8, 50), RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$", ErrorMessage ="Needs: 8 to 50 characters. An uppercase character(ABC). A number(123)")]
    string Password,
    [DataType(DataType.Password), Length(8, 20)] string ConfirmPassword,
    [Length(1, 50)] string KnownAs,
    [Range(typeof(DateOnly), "1900-01-01", "2050-01-01")] DateOnly DateOfBirth,
    string Gender,
    [Length(3, 30)] string Country,
    [Length(3, 30)] string State,
    [Length(3, 30)] string City,
    [MaxLength(1000)] string? Introduction,
    [MaxLength(1000)] string? LookingFor,
    [MaxLength(1000)] string? Interests
);

