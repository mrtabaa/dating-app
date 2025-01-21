using api.DTOs.helpers;

namespace api.Controllers.Helpers;

public class SeedUsersController(IMongoClient client, IMyMongoDbSettings dbSettings, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    : BaseApiController
{
    #region Db Settings

    private readonly IMongoDatabase _database = client.GetDatabase(dbSettings.DatabaseName);

    #endregion

    #region Add Dummy users to DB

    [HttpPost]
    public async Task<ActionResult<IEnumerable<MemberDto?>>> CreateDummyMembers(IEnumerable<DummyRegisterDto> inputUsersDummy)
    {
        #region If databaseExists

        // check if database already exists using its status
        // https://stackoverflow.com/a/53803908/3944285
        var command = "{ dbStats: 1, scale: 1 }";
        var dbStats = await _database.RunCommandAsync<BsonDocument>(command);
        bool databaseExists;

        if (dbStats["collections"].BsonType == BsonType.Int64)
        {
            long collectionsCount = dbStats["collections"].AsInt64;
            databaseExists = collectionsCount > 0 || dbStats["indexes"].AsInt64 > 0;
        }
        else
        {
            int collectionsCount = dbStats["collections"].AsInt32;
            databaseExists = collectionsCount > 0 || dbStats["indexes"].AsInt32 > 0;
        }

        if (databaseExists)
            // return BadRequest("Database already exists");
            // await _database.DropCollectionAsync(_collectionName);
            await client.DropDatabaseAsync("dating-app");

        #endregion

        #region Import db seed

        // add each user to DB
        List<AppUser> appUsers = [];

        #region Roles Management

        AppRole[] roles = AppVariablesExtensions.Roles;

        foreach (AppRole role in roles) await roleManager.CreateAsync(role);

        foreach (DummyRegisterDto userInput in inputUsersDummy)
        {
            AppUser appUser = Mappers.ConvertDummyRegisterDtoToAppUser(userInput);

            await userManager.CreateAsync(appUser, userInput.Password);

            await userManager.AddToRoleAsync(appUser, Roles.Member.ToString());

            appUsers.Add(appUser);
        }

        #region Admin & Moderator
        
        AppUser admin = new()
        {
            Email = "admin@a.com",
            UserName = "admin",
            IsProfileCompleted = true
        };

        await userManager.CreateAsync(admin, "Aaaaaaa/1");
        await userManager.AddToRolesAsync(admin, [Roles.Admin.ToString().ToUpper(), Roles.Moderator.ToString().ToUpper()]);
        string verifyToken = await userManager.GenerateEmailConfirmationTokenAsync(admin);
        await userManager.ConfirmEmailAsync(admin, verifyToken);

        AppUser moderator = new()
        {
            Email = "moderator@a.com",
            UserName = "moderator",
            IsProfileCompleted = true
        };

        await userManager.CreateAsync(moderator, "Aaaaaaa/1");
        await userManager.AddToRolesAsync(moderator, [Roles.Moderator.ToString().ToUpper()]);
        verifyToken = await userManager.GenerateEmailConfirmationTokenAsync(moderator);
        await userManager.ConfirmEmailAsync(moderator, verifyToken);
        
        #endregion

        #endregion Roles Management

        // convert AppUser to MemberDto
        List<MemberDto?> memberDtos = [];
        foreach (AppUser appuser in appUsers) memberDtos.Add(Mappers.ConvertAppUserToMemberDto(appuser));

        return memberDtos;

        #endregion
    }

    #endregion
}