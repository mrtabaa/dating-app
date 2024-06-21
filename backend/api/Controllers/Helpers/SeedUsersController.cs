namespace api.Controllers;

public class SeedUsersController : BaseApiController
{
    #region Db Settings
    private readonly IMongoDatabase _database;
    private readonly IMongoClient _client;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public SeedUsersController(IMongoClient client, IMyMongoDbSettings dbSettings, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        _database = client.GetDatabase(dbSettings.DatabaseName);
        _client = client;

        _userManager = userManager;
        _roleManager = roleManager;
    }
    #endregion

    #region Add Dummy users to DB
    [HttpPost]
    public async Task<ActionResult<IEnumerable<MemberDto?>>> CreateDummyMembers(IEnumerable<DummyRegisterDto> inputUsersDummy)
    {
        #region If databaseExists
        // check if database already exists using its status.
        // https://stackoverflow.com/a/53803908/3944285
        var command = "{ dbStats: 1, scale: 1 }";
        var dbStats = await _database.RunCommandAsync<BsonDocument>(command);
        bool databaseExists;

        if (dbStats["collections"].BsonType == BsonType.Int64)
        {
            var collectionsCount = dbStats["collections"].AsInt64;
            databaseExists = collectionsCount > 0 || dbStats["indexes"].AsInt64 > 0;
        }
        else
        {
            var collectionsCount = dbStats["collections"].AsInt32;
            databaseExists = collectionsCount > 0 || dbStats["indexes"].AsInt32 > 0;
        }

        if (databaseExists == true)
            // return BadRequest("Database already exists");
            // await _database.DropCollectionAsync(_collectionName);
            await _client.DropDatabaseAsync("dating-app");
        #endregion

        #region Import db seed
        // add each user to DB
        List < AppUser > appUsers = [];

        #region Roles Management
        AppRole[] roles = AppVariablesExtensions.roles;

        foreach (AppRole role in roles)
        {
            await _roleManager.CreateAsync(role);
        }

        foreach (var userInput in inputUsersDummy)
        {
            AppUser appUser = Mappers.ConvertDummyRegisterDtoToAppUser(userInput);

            await _userManager.CreateAsync(appUser, userInput.Password);

            await _userManager.AddToRoleAsync(appUser, Roles.member.ToString());

            appUsers.Add(appUser);
        }

        AppUser admin = new()
        {
            Email = "admin@a.com",
            UserName = "admin",
            IsProfileCompleted = true
        };

        await _userManager.CreateAsync(admin, "Aaaaaaa1");
        await _userManager.AddToRolesAsync(admin, [Roles.admin.ToString(), Roles.moderator.ToString()]);

        AppUser moderator = new()
        {
            Email = "moderator@a.com",
            UserName = "moderator",
            IsProfileCompleted = true
        };

        await _userManager.CreateAsync(moderator, "Aaaaaaa1");
        await _userManager.AddToRolesAsync(moderator, [Roles.moderator.ToString()]);

        #endregion Roles Management

        // convert AppUser to MemberDto
        List<MemberDto?> memberDtos = [];
        foreach (AppUser appuser in appUsers)
        {
            memberDtos.Add(Mappers.ConvertAppUserToMemberDto(appuser));
        }

        return memberDtos;
        #endregion
    }
    #endregion
}
