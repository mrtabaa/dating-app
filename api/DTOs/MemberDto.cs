namespace api.DTOs;

public record MemberDto(
    string Schema,
    string Id,
    [MinLength(2), MaxLength(20)] string Name,
    [EmailAddress] string Email,
    [Range(16, 99)] int Age,
    string KnownAs,
    DateTime Created,
    DateTime LastActive,
    string Gender,
    string Introduction,
    string LookingFor,
    string Interests,
    string City,
    string Country,
    IEnumerable<PhotoDto>? Photos
);
