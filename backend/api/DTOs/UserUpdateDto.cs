namespace api.DTOs;

public record UserUpdateDto(
    string? Schema,
    [Length(1, 50)] string? KnownAs,
    [MaxLength(1000)] string? Introduction,
    [MaxLength(1000)] string? LookingFor,
    [MaxLength(1000)] string? Interests,
    [MinLength(2)] string CountryAcr,
    [Length(3, 30)] string Country,
    [Length(3, 30)] string State,
    [Length(3, 30)] string City,
    [Optional] bool IsProfileCompleted
);