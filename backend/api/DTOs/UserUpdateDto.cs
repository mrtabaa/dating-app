namespace api.DTOs;

public record UserUpdateDto(
    string? Schema,
    [MaxLength(500)] string? Introduction,
    [MaxLength(500)] string? LookingFor,
    [MaxLength(500)] string? Interests,
    [MinLength(3), MaxLength(30)] string City,
    [MinLength(3), MaxLength(30)] string Country
);