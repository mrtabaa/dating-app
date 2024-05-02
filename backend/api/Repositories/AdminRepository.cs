
namespace api.Repositories;

public class AdminRepository : IAdminRepository
{
    #region Db and Token Settings
    private readonly IMongoCollection<AppUser>? _collection;
    private readonly UserManager<AppUser> _userManager;

    // constructor - dependency injection
    public AdminRepository(IMongoClient client, IMyMongoDbSettings dbSettings, UserManager<AppUser> userManager, ITokenService tokenService)
    {
        var database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AppUser>(AppVariablesExtensions.collectionUsers);

        _userManager = userManager;
    }
    #endregion

    #region CRUD
    public async Task<IEnumerable<UserWithRoleDto>> GetUsersWithRolesAsync()
    {
        List<UserWithRoleDto> usersWithRoles = [];

        foreach (AppUser appUser in _userManager.Users)
        {
            var roles = await _userManager.GetRolesAsync(appUser);

            usersWithRoles.Add(
                new UserWithRoleDto(
                    UserName: appUser.UserName!,
                    Roles: roles
                )
            );
        }

        return usersWithRoles;
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
