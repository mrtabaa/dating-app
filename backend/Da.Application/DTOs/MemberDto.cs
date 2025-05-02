namespace Da.Application.DTOs;

public record MemberDto(
    string Schema,
    string? UserName,
    int Age,
    DateOnly DateOfBirth,
    string KnownAs,
    DateTimeOffset Created,
    DateTimeOffset LastActive,
    string Gender,
    string? Introduction,
    string? LookingFor,
    string? Interests,
    string CountryAcr,
    string Country,
    string State,
    string City,
    List<Photo> Photos,
    bool IsFollowing
);