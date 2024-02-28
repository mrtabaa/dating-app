
namespace api.Repositories;

public class AdminRepository : IAdminRepository
{
    #region Db and Token Settings
    private readonly IMongoCollection<AppUser>? _collection;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService; // save user credential as a token

    // constructor - dependency injection
    public AdminRepository(IMongoClient client, IMyMongoDbSettings dbSettings, UserManager<AppUser> userManager, ITokenService tokenService)
    {
        var database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AppUser>(AppVariablesExtensions.collectionUsers);

        _userManager = userManager;
        _tokenService = tokenService;
    }
    #endregion

    #region CRUD
    public async Task<IEnumerable<MemberWithRoleDto>> GetUsersWithRolesAsync()
    {
        List<MemberWithRoleDto> users = [];

        await Task.Run(() =>
        {
            _userManager.Users.ToList().ForEach(appUser =>
               users.Add(
                   new MemberWithRoleDto(
                       UserName: appUser.UserName!,
                       Roles: _userManager.GetRolesAsync(appUser).Result
                   )
               )
           );
        });

        return users;
    }

    public async Task<IEnumerable<string>?> EditMemberRole(MemberWithRoleDto memberWithRoleDto)
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

    #endregion
}
