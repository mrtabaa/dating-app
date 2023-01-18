namespace api.DTOs;

public record UserDto(
    string? Id,
    [MinLength(7), MaxLength(20)] string Name,
    [EmailAddress] string Email
);
