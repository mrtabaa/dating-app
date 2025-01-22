namespace api.Repositories;

public class AdminRepository : IAdminRepository
{
    #region Db and Token Settings

    private readonly IMongoCollection<AppUser>? _collection;
    private readonly UserManager<AppUser> _userManager;

    // constructor - dependency injection
    public AdminRepository(IMongoClient client, IMyMongoDbSettings dbSettings, UserManager<AppUser> userManager)
    {
        IMongoDatabase dbName = client.GetDatabase(dbSettings.DatabaseName)
                                ?? throw new ArgumentNullException(nameof(dbName), "The database name cannot be null.");
        _collection = dbName.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);

        _userManager = userManager;
    }

    #endregion

    #region CRUD

    public async Task<OperationResult<PagedList<AppUser>>> GetUsersWithRolesAsync(AdminParams adminParams, CancellationToken cancellationToken)
    {
        IMongoQueryable<AppUser> query = _collection.AsQueryable();

        if (!string.IsNullOrEmpty(adminParams.Search))
        {
            query = query.Where(user =>
                (user.NormalizedUserName != null && user.NormalizedUserName.Contains(adminParams.Search, StringComparison.CurrentCultureIgnoreCase))
                || (user.NormalizedEmail != null && user.NormalizedEmail.Contains(adminParams.Search, StringComparison.CurrentCultureIgnoreCase)));
        }

        // set no filter in AsQueryable(). Send a plain query
        return new OperationResult<PagedList<AppUser>>(
            true,
            await PagedList<AppUser>.CreatePagedListAsync(query, adminParams.PageNumber, adminParams.PageSize, cancellationToken)
        );
    }

    public async Task<OperationResult<IEnumerable<string>>> EditMemberRole(UserWithRoleDto memberWithRoleDto)
    {
        AppUser? appUser = await _userManager.FindByNameAsync(memberWithRoleDto.UserName.ToUpper());

        if (appUser is null)
            return new OperationResult<IEnumerable<string>>(false);

        IEnumerable<string> userRoles = _userManager.GetRolesAsync(appUser).Result;

        // Add selected roles
        IdentityResult? result = await _userManager.AddToRolesAsync(appUser, memberWithRoleDto.Roles.Except(userRoles));

        if (!result.Succeeded)
            return new OperationResult<IEnumerable<string>>(false);

        // Delete non-selected roles
        result = await _userManager.RemoveFromRolesAsync(appUser, userRoles.Except(memberWithRoleDto.Roles));

        if (!result.Succeeded)
            return new OperationResult<IEnumerable<string>>(false);

        return new OperationResult<IEnumerable<string>>(
            true,
            await _userManager.GetRolesAsync(appUser)
        );
    }

    public async Task<bool> VerifyByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        AppUser? appUser = await _userManager.FindByNameAsync(username);
        if (appUser is null || string.IsNullOrEmpty(appUser.Email)) return false;

        string verificationCode = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);

        IdentityResult result = await _userManager.ConfirmEmailAsync(appUser, verificationCode);
        return result.Succeeded;
    }

    public async Task<AppUser?> DeleteMemberAsync(string userName) =>
        await _collection.FindOneAndDeleteAsync(
            user => user.NormalizedUserName != "ADMIN" && user.NormalizedUserName == userName.ToUpper());

    public async Task<UpdateResult> ResetConnectionsPresenceAsync(CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser> updateDefinition = Builders<AppUser>.Update
            .Set(appUser => appUser.ConnectionsPresence, []);

        return await _collection.UpdateManyAsync(
            appUser => !appUser.Id.Equals(ObjectId.Empty), updateDefinition, null, cancellationToken);
    }

    public async Task<UpdateResult> ResetGroupNamesAsync(CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser> updateDefinition = Builders<AppUser>.Update
            .Set(appUser => appUser.MessageGroups, []);

        return await _collection.UpdateManyAsync(
            appUser => !appUser.Id.Equals(ObjectId.Empty), updateDefinition, null, cancellationToken);
    }

    #endregion
}