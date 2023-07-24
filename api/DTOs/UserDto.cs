namespace api.DTOs;

public record UserDto(
    string Schema,
    string? Id,
    string? Token,
    string? Name,
    string? Email,
    string? ProfilePhotoUrl
);
