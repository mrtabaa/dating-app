namespace api.Repositories;

public class AccountRepository : IAccountRepository
{

    #region Db and Token Settings
    const string _collectionName = "users";
    private readonly IMongoCollection<AppUser>? _collection;
    private readonly ITokenService _tokenService; // save user credential as a token

    // constructor - dependency injection
    public AccountRepository(IMongoClient client, IMongoDbSettings dbSettings, ITokenService tokenService)
    {
        var database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AppUser>(_collectionName);
        _tokenService = tokenService;
    }
    #endregion

    #region CRUD
    public async Task<LoggedInDto?> CreateAsync(UserRegisterDto userInput, CancellationToken cancellationToken)
    {
        bool userExist = await _collection.Find<AppUser>(appUser => appUser.Email == userInput.Email).AnyAsync(cancellationToken);

        if (userExist) return null;

        AppUser appUser = Mappers.ConvertUserRegisterDtoToAppUser(userInput);

        // Insertion successful OR throw an exception
        await _collection!.InsertOneAsync(appUser); // mark ! after _collection! tells compiler it's nullable

        if (appUser.Id is null)
            _ = appUser.Id ?? throw new ArgumentException("appUser.Id cannot be null", nameof(appUser.Id));

        return Mappers.ConvertAppUserToLoggedInDto(appUser, _tokenService.CreateToken(appUser));
    }

    public async Task<LoggedInDto?> LoginAsync(LoginDto userInput, CancellationToken cancellationToken)
    {
        var appUser = await _collection.Find<AppUser>(appUser => appUser.Email == userInput.Email.ToLower().Trim()).FirstOrDefaultAsync(cancellationToken);

        if (appUser is null) return null;

        if (appUser.Id is null)
            _ = appUser.Id ?? throw new ArgumentException("appUser.Id cannot be null", nameof(appUser.Id));

        using var hmac = new HMACSHA512(appUser.PasswordSalt!);
        var ComputedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userInput.Password));

        if (appUser.PasswordHash is not null && appUser.PasswordHash.SequenceEqual(ComputedHash))
        {
            UpdateLastActiveInDb(appUser, cancellationToken);

            return Mappers.ConvertAppUserToLoggedInDto(appUser, _tokenService.CreateToken(appUser));
        }

        return null;
    }
    #endregion CRUD

    private async void UpdateLastActiveInDb(AppUser appUserIn, CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser> newLastActive = Builders<AppUser>.Update.Set(appUser =>
                       appUser.LastActive, DateTime.UtcNow);

        await _collection.UpdateOneAsync(appUser =>
        appUser.Id == appUserIn.Id, newLastActive, null, cancellationToken);
    }
}
