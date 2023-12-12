namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedUsersController : BaseApiController
{
    #region Db Settings
    private readonly IMongoDatabase _database;
    const string _collectionName = "users";
    private readonly IMongoCollection<AppUser>? _collection;

    public SeedUsersController(IMongoClient client, IMongoDbSettings dbSettings)
    {
        _database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = _database.GetCollection<AppUser>(_collectionName);
    }
    #endregion

    #region Add Dummy users to DB
    [HttpPost]
    public async Task<ActionResult<IEnumerable<UserDto?>>> CreateDummyUsers(IEnumerable<UserRegisterDto> inputUsersDummy)
    {
        #region If databaseExists
        // check if database already exists using its status
        // https://stackoverflow.com/a/53803908/3944285
        var command = "{ dbStats: 1, scale: 1 }";
        var dbStats = await _database.RunCommandAsync<BsonDocument>(command);
        var databaseExists = dbStats["collections"].AsInt32 > 0 || dbStats["indexes"].AsInt32 > 0;

        Console.WriteLine(databaseExists);

        if (databaseExists == true)
            return BadRequest("Database already exists");
        // await _database.DropCollectionAsync(_collectionName);
        #endregion

        #region Import db seed

        // manually dispose HMACSHA512 after being done
        using var hmac = new HMACSHA512();

        // add each user to DB
        foreach (var userInput in inputUsersDummy)
        {
            AppUser appUser = new(
                Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                Id: null,
                Email: userInput.Email.ToLower(),
                PasswordHash: hmac.ComputeHash(Encoding.UTF8.GetBytes(userInput.Password!)),
                PasswordSalt: hmac.Key,
                DateOfBirth: userInput.DateOfBirth,
                KnownAs: userInput.KnownAs,
                Created: DateTime.UtcNow,
                LastActive: DateTime.UtcNow,
                Gender: userInput.Gender,
                Introduction: userInput.Introduction,
                LookingFor: userInput.LookingFor,
                Interests: userInput.Interests,
                City: userInput.City,
                Country: userInput.Country,
                Photos: userInput.Photos!
            );

            await _collection!.InsertOneAsync(appUser);
        }

        // get all users from DB
        IEnumerable<AppUser> appUsers = await _collection.Find(new BsonDocument()).ToListAsync();

        // convert AppUser to UserDto
        List<UserDto?> userDtos = new();
        foreach (AppUser appuser in appUsers)
        {
            userDtos.Add(Mappers.GenerateUserDto(appuser));
        }

        return userDtos;
        #endregion
    }
    #endregion
}
