namespace api.DTOs;

public record UserUpdateDto(
    string? Schema,
    [MaxLength(1000)] string? Introduction,
    [MaxLength(1000)] string? LookingFor,
    [MaxLength(1000)] string? Interests,
    [MinLength(3), MaxLength(30)] string City,
    [MinLength(3), MaxLength(30)] string Country
);