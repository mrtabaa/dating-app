namespace api.DTOs;

public record MemberWithRoleDto(
    string Id,
    string UserName,
    IEnumerable<string> Roles
);
