namespace api.DTOs;

public record MemberUpdateDto(
    string? Schema,
    [MinLength(10), MaxLength(500)] string Introduction,
    [MinLength(10), MaxLength(500)] string LookingFor,
    [MinLength(10), MaxLength(500)] string Interests,
    [MinLength(3), MaxLength(30)] string City,
    [MinLength(3), MaxLength(30)] string Country
);