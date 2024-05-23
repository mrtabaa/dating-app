namespace api.DTOs;

public record UserUpdateDto(
    string? Schema,
    [Length(1, 50)] string? KnownAs,
    [MaxLength(1000)] string? Introduction,
    [MaxLength(1000)] string? LookingFor,
    [MaxLength(1000)] string? Interests,
    [MinLength(3), MaxLength(30)] string Country,
    [MinLength(3), MaxLength(30)] string State,
    [MinLength(3), MaxLength(30)] string City
);