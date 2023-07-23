namespace api.DTOs;

public record UserDto(
    string Schema,
    string? Token,
    string? Name,
    string? Email,
    string? ProfilePhotoUrl
);
