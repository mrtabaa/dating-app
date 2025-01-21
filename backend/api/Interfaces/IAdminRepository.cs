namespace api.Interfaces;

public interface IAdminRepository
{
    public Task<PagedList<AppUser>> GetUsersWithRolesAsync(AdminParams adminParams, CancellationToken cancellationToken);
    public Task<IEnumerable<string>?> EditMemberRole(UserWithRoleDto memberWithRoleDto);
    public Task<bool> VerifyByUsernameAsync(string username, CancellationToken cancellationToken);
    public Task<AppUser?> DeleteMemberAsync(string userName);
    public Task<UpdateResult> ResetConnectionsPresenceAsync(CancellationToken cancellationToken);
    public Task<UpdateResult> ResetGroupNamesAsync(CancellationToken cancellationToken);
}