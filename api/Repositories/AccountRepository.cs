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
    public async Task<UserDto?> Create(UserRegisterDto userInput, CancellationToken cancellationToken)
    {
        bool userExist = await _collection.AsQueryable().Where<AppUser>(user => user.Email == userInput.Email).AnyAsync(cancellationToken);

        if (userExist) return null;

        AppUser appUser = Mappers.GenerateAppUser(userInput);

        // if insertion successful OR throw an exception
        await _collection!.InsertOneAsync(appUser); // mark ! after _collection! tells compiler it's nullable

        if (appUser.Id is null)
            _ = appUser.Id ?? throw new ArgumentException("appUser.Id cannot be null", nameof(appUser.Id));

        return new UserDto(
            Schema: AppVariablesExtensions.AppVersions.Last<string>(),
            Id: appUser.Id,
            Token: _tokenService.CreateToken(appUser),
            Email: appUser.Email,
            KnownAs: appUser.KnownAs,
            ProfilePhotoUrl: null
        );
    }

    public async Task<UserDto?> Login(LoginDto userInput, CancellationToken cancellationToken)
    {
        var appUser = await _collection.Find<AppUser>(user => user.Email == userInput.Email.ToLower().Trim()).FirstOrDefaultAsync(cancellationToken);

        if (appUser is null) return null;

        using var hmac = new HMACSHA512(appUser.PasswordSalt!);

        if (appUser.Id is null)
            _ = appUser.Id ?? throw new ArgumentException("appUser.Id cannot be null", nameof(appUser.Id));

        var ComputedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userInput.Password!));
        if (appUser.PasswordHash is not null && appUser.PasswordHash.SequenceEqual(ComputedHash))
            return new UserDto(
                Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                Id: appUser.Id,
                Token: _tokenService.CreateToken(appUser),
                Email: appUser.Email,
                KnownAs: appUser.KnownAs,
                ProfilePhotoUrl: appUser.Photos.FirstOrDefault(photo => photo.IsMain)?.Url_128
            );

        _ = appUser ?? throw new ArgumentException("valid userInput but user was not created", nameof(appUser));
        return null;
    }
    #endregion CRUD
}
