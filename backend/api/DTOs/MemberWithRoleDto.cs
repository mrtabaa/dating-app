namespace api.DTOs;

public record MemberWithRoleDto(
    string UserName,
    IEnumerable<string> Roles
);
