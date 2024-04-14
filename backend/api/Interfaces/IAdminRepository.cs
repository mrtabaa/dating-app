namespace api.Interfaces;

public interface IAdminRepository
{
    public Task<IEnumerable<MemberWithRoleDto>> GetUsersWithRolesAsync();
    public Task<IEnumerable<string>?> EditMemberRole(MemberWithRoleDto memberWithRoleDto);
    public Task<AppUser?> DeleteUserAsync(string userName);
}
