namespace api.Interfaces;

public interface IAdminRepository
{
    public Task<IEnumerable<MemberWithRoleDto>> GetUsersWithRolesAsync();
    public Task<IEnumerable<string>?> EditMemberRole(string userName, string newRoles);
}
