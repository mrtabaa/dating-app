namespace api.Interfaces;

public interface IAdminRepository
{
    public Task<PagedList<AppUser>> GetUsersWithRolesAsync(AdminParams adminParams, CancellationToken cancellationToken);
    public Task<IEnumerable<string>?> EditMemberRole(UserWithRoleDto memberWithRoleDto);
    public Task<AppUser?> DeleteMemberAsync(string userName);
}
