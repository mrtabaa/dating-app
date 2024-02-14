namespace api.Repositories;

public class AccountRepository : IAccountRepository
{

    #region Db and Token Settings
    private readonly IMongoCollection<AppUser>? _collection;
    private readonly ITokenService _tokenService; // save user credential as a token

    // constructor - dependency injection
    public AccountRepository(IMongoClient client, IMongoDbSettings dbSettings, ITokenService tokenService)
    {
        var database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AppUser>(AppVariablesExtensions.collectionUsers);
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
        await _collection!.InsertOneAsync(appUser, null, cancellationToken); // mark ! after _collection! tells compiler it's nullable

        if (appUser.Email is null)
            _ = appUser.Email ?? throw new ArgumentException("appUser.Email cannot be null", nameof(appUser.Email));

        return Mappers.ConvertAppUserToLoggedInDto(appUser, _tokenService.CreateToken(appUser));
    }

    public async Task<LoggedInDto?> LoginAsync(LoginDto userInput, CancellationToken cancellationToken)
    {
        var appUser = await _collection.Find<AppUser>(appUser => appUser.Email == userInput.Email.ToLower().Trim()).FirstOrDefaultAsync(cancellationToken);

        if (appUser is null) return null;

        if (appUser.Email is null)
            _ = appUser.Email ?? throw new ArgumentException("appUser.Id cannot be null", nameof(appUser.Email));

        using var hmac = new HMACSHA512(appUser.PasswordSalt!);
        var ComputedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userInput.Password));

        if (appUser.PasswordHash is not null && appUser.PasswordHash.SequenceEqual(ComputedHash))
        {
            return Mappers.ConvertAppUserToLoggedInDto(appUser, _tokenService.CreateToken(appUser));
        }

        return null;
    }

    public async Task<LoggedInDto?> GetLoggedInUserAsync(string? userEmail, string? token, CancellationToken cancellationToken)
    {
        if (!(userEmail is null || token is null))
        {
            AppUser appUser = await _collection.Find<AppUser>(appUser => appUser.Email == userEmail).FirstOrDefaultAsync(cancellationToken);

            return appUser is null ? null : Mappers.ConvertAppUserToLoggedInDto(appUser, token);
        }

        return null;
    }

    public async Task<DeleteResult?> DeleteUserAsync(string? userEmail, CancellationToken cancellationToken) =>
       await _collection.DeleteOneAsync<AppUser>(appUser => appUser.Email == userEmail, cancellationToken);
    #endregion CRUD
}
