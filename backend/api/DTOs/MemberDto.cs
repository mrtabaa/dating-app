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
    string Country,
    string State,
    string City,
    List<Photo> Photos,
    bool IsFollowing
);
