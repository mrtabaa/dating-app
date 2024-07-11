
namespace api.Repositories;

public class AdminRepository : IAdminRepository
{
    #region Db and Token Settings
    private readonly IMongoCollection<AppUser>? _collection;
    private readonly UserManager<AppUser> _userManager;

    // constructor - dependency injection
    public AdminRepository(IMongoClient client, IMyMongoDbSettings dbSettings, UserManager<AppUser> userManager)
    {
        var database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AppUser>(AppVariablesExtensions.collectionUsers);

        _userManager = userManager;
    }
    #endregion

    #region CRUD
    public async Task<PagedList<AppUser>> GetUsersWithRolesAsync(AdminParams adminParams, CancellationToken cancellationToken)
    {
        IMongoQueryable<AppUser> query = _collection.AsQueryable();

        if (!string.IsNullOrEmpty(adminParams.Search))
            query = query.Where(user => 
                user.NormalizedUserName != null && user.NormalizedUserName.Contains(adminParams.Search, StringComparison.CurrentCultureIgnoreCase)
                || user.NormalizedEmail != null && user.NormalizedEmail.Contains(adminParams.Search, StringComparison.CurrentCultureIgnoreCase));

        // set no filter in AsQueryable(). Send a plain query
        return await PagedList<AppUser>.CreatePagedListAsync(query, adminParams.PageNumber, adminParams.PageSize, cancellationToken);
    }

    public async Task<IEnumerable<string>?> EditMemberRole(UserWithRoleDto memberWithRoleDto)
    {
        AppUser? appUser = await _userManager.FindByNameAsync(memberWithRoleDto.UserName.ToUpper());

        if (appUser is null) return null;

        IEnumerable<string> userRoles = _userManager.GetRolesAsync(appUser).Result;

        // Add selected roles
        IdentityResult? result = await _userManager.AddToRolesAsync(appUser, memberWithRoleDto.Roles.Except(userRoles));

        if (!result.Succeeded) return null;

        // Delete non-selected roles
        result = await _userManager.RemoveFromRolesAsync(appUser, userRoles.Except(memberWithRoleDto.Roles));

        if (!result.Succeeded) return null;

        return await _userManager.GetRolesAsync(appUser);
    }

    public async Task<AppUser?> DeleteMemberAsync(string userName) =>
        await _collection.FindOneAndDeleteAsync(user => user.NormalizedUserName != "ADMIN" && user.NormalizedUserName == userName.ToUpper());

    #endregion
}
