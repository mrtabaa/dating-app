using Microsoft.AspNetCore.Identity;

namespace api.Controllers;

public class SeedUsersController : BaseApiController
{
    #region Db Settings
    private readonly IMongoDatabase _database;
    const string _collectionName = "users";
    private readonly IMongoCollection<AppUser>? _collection;
    private readonly UserManager<AppUser> _userManager;

    public SeedUsersController(IMongoClient client, IMyMongoDbSettings dbSettings, UserManager<AppUser> userManager)
    {
        _database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = _database.GetCollection<AppUser>(_collectionName);

        _userManager = userManager;
    }
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
            var collectionsCount = dbStats["collections"].AsInt64;
            databaseExists = collectionsCount > 0 || dbStats["indexes"].AsInt64 > 0;
        }
        else
        {
            var collectionsCount = dbStats["collections"].AsInt32;
            databaseExists = collectionsCount > 0 || dbStats["indexes"].AsInt32 > 0;
        }

        if (databaseExists == true)
            return BadRequest("Database already exists");
        // await _database.DropCollectionAsync(_collectionName);
        #endregion

        #region Import db seed
        // add each user to DB
        List<AppUser> appUsers = [];

        foreach (var userInput in inputUsersDummy)
        {
            AppUser appUser = Mappers.ConvertDummyRegisterDtoToAppUser(userInput);

            await _userManager.CreateAsync(appUser, userInput.Password);

            appUsers.Add(appUser);
        }

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
