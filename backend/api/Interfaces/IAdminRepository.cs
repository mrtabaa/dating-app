namespace api.Interfaces;

public interface IAdminRepository
{
    public Task<IEnumerable<UserWithRoleDto>> GetUsersWithRolesAsync();
    public Task<IEnumerable<string>?> EditMemberRole(UserWithRoleDto memberWithRoleDto);
    public Task<AppUser?> DeleteMemberAsync(string userName);
}
