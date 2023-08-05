namespace api.DTOs;

public record UserDto(
    string Schema,
    string Id,
    string Token,
    string KnownAs,
    string Email,
    string? ProfilePhotoUrl
);
