namespace da.Application.DTOs;

public record UserWithRoleDto(
    string UserName,
    IEnumerable<string> Roles
);