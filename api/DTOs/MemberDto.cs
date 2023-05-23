namespace api.DTOs;

public record MemberDto(
    string Schema,
    string Id,
    [MinLength(2), MaxLength(20)] string Name,
    [EmailAddress] string Email,
    [Range(16, 99)] int Age,
    [MinLength(3), MaxLength(20)] string KnownAs,
    DateTime Created,
    DateTime LastActive,
    string Gender,
    [MinLength(10), MaxLength(500)] string Introduction,
    [MinLength(10), MaxLength(500)] string LookingFor,
    [MinLength(10), MaxLength(500)] string Interests,
    [MinLength(3), MaxLength(20)] string City,
    [MinLength(3), MaxLength(20)] string Country,
    IEnumerable<PhotoDto>? Photos
);
