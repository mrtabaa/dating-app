namespace api.DTOs;

public record MemberDto(
    string Schema,
    string Id,
    [MinLength(2), MaxLength(20)] string Name,
    [EmailAddress] string Email,
    int Age,
    string KnownAs,
    DateTime Created,
    DateTime LastActive,
    string Gender,
    string Introduction,
    string LookingFor,
    string Interests,
    string City,
    string Country,
    List<PhotoDto>? Photos
);
