namespace api.DTOs;

public record UserDto(
    string? Id,
    [EmailAddress] string Email
);
