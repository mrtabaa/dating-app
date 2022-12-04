using System.Data.Common;
using ZstdSharp;

namespace api.Repositories;
public class AccountRepository : IAccountRepository {

    #region Db and Token Settings
    const string _collectionName = "Users";
    private readonly IMongoCollection<AppUser>? _collection;
    private readonly ITokenService _tokenService; // save user credential as a token
    private readonly CancellationToken _cancellationToken;


    // constructor - dependency injection
    public AccountRepository(IMongoClient client, IMongoDbSettings dbSettings, ITokenService tokenService) {
        var database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AppUser>(_collectionName);
        _tokenService = tokenService;
        _cancellationToken = new CancellationToken();
    }
    #endregion

    #region CRUD
    public async Task<LoginSuccessDto?> Create(UserRegisterDto userInput) {
        if (await CheckEmailExist(userInput.Email.ToLower()))
            return null;

        // manually dispose HMACSHA512 after being done
        using var hmac = new HMACSHA512();

        var user = new AppUser {
            Schema = 0,
            Email = userInput.Email,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userInput.Password!)),
            PasswordSalt = hmac.Key,
        };

        // if insertion successful OR throw an exception
        await _collection!.InsertOneAsync(user); // mark ! after _collection! tells compiler it's nullable

        return new LoginSuccessDto {
            Token = _tokenService.CreateToken(user),
            Email = user.Email
        };

    }

    public async Task<LoginSuccessDto?> Login(LoginDto userInput) {
        var user = await _collection.Find<AppUser>(user => user.Email == userInput.Email.ToLower()).FirstOrDefaultAsync(_cancellationToken);

        if (user == null)
            return null;

        using var hmac = new HMACSHA512(user.PasswordSalt!);

        var ComputedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userInput.Password!));
        if (user.PasswordHash != null && user.PasswordHash.SequenceEqual(ComputedHash))
            return new LoginSuccessDto {
                Token = _tokenService.CreateToken(user),
                Email = user.Email
            };

        _ = user ?? throw new ArgumentException("valid userInput but user was not created", nameof(user));
        return null;
    }
    #endregion CRUD

    #region Helper methods
    private async Task<bool> CheckEmailExist(string email) {
        return null != await _collection.Find<AppUser>(user => user.Email == email).FirstOrDefaultAsync()
            ? true : false;
    }
    #endregion Helper methods
}
