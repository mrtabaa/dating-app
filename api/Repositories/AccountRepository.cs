using System.Text.RegularExpressions;

namespace api.Repositories;

public class AccountRepository : IAccountRepository
{

    #region Db and Token Settings
    const string _collectionName = "users";
    private readonly IMongoCollection<AppUser>? _collection;
    private readonly ITokenService _tokenService; // save user credential as a token
    private readonly CancellationToken _cancellationToken;


    // constructor - dependency injection
    public AccountRepository(IMongoClient client, IMongoDbSettings dbSettings, ITokenService tokenService)
    {
        var database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AppUser>(_collectionName);
        _tokenService = tokenService;
        _cancellationToken = new CancellationToken();
    }
    #endregion

    #region CRUD
    public async Task<LoginSuccessDto?> Create(UserRegisterDto userInput)
    {
        AppUser appUser = Mappers.GenerateAppUser(userInput);

        // if insertion successful OR throw an exception
        await _collection!.InsertOneAsync(appUser); // mark ! after _collection! tells compiler it's nullable

        return new LoginSuccessDto(
            Schema: AppVariablesExtensions.AppVersions.Last<string>(),
            Token: _tokenService.CreateToken(appUser),
            Name: appUser.Name,
            Email: appUser.Email
        );
    }

    public async Task<LoginSuccessDto?> Login(LoginDto userInput)
    {
        var user = await _collection.Find<AppUser>(user => user.Email == userInput.Email.ToLower()).FirstOrDefaultAsync(_cancellationToken);

        if (user == null)
            return null;

        using var hmac = new HMACSHA512(user.PasswordSalt!);

        var ComputedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userInput.Password!));
        if (user.PasswordHash != null && user.PasswordHash.SequenceEqual(ComputedHash))
            return new LoginSuccessDto(
                Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                Token: _tokenService.CreateToken(user),
                Name: user.Name,
                Email: user.Email
            );

        _ = user ?? throw new ArgumentException("valid userInput but user was not created", nameof(user));
        return null;
    }
    #endregion CRUD
}
