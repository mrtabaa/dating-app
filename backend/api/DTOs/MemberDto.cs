namespace api.DTOs;

public record MemberDto(
    string Schema,
    string? UserName,
    int Age,
    DateOnly DateOfBirth,
    string KnownAs,
    DateTime Created,
    DateTime LastActive,
    string Gender,
    string? Introduction,
    string? LookingFor,
    string? Interests,
    string City,
    string Country,
    List<Photo> Photos,
    bool IsFollowing
);
