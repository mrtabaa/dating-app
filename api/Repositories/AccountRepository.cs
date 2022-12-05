using System.Data.Common;
using System.Text.RegularExpressions;
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
        
        if (!validateEmailPattern(userInput.Email))
            return new LoginSuccessDto { BadEmailPattern = true };

        string lowercaseEmail = userInput.Email.ToLower();

        if (await CheckEmailExist(lowercaseEmail))
            return null;

        // manually dispose HMACSHA512 after being done
        using var hmac = new HMACSHA512();

        var user = new AppUser {
            Schema = 0,
            Email = lowercaseEmail,
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
    private async Task<bool> CheckEmailExist(string email) =>
        null != await _collection.Find<AppUser>(user => user.Email == email).FirstOrDefaultAsync()
        ? true : false;

/// <summary>
/// * TLD support from 2 to 5 chars (modify the values as you want)
/// * Supports: abc@gmail.com.us
/// * Non-sensitive case 
/// * Stops operation if takes longer than 250ms and throw a detailed exception
/// </summary>
/// <param name="email"></param>
/// <returns>success: true | fail: false </returns>
/// <exception cref="ArgumentException"></exception>
    private bool validateEmailPattern(string email) {
        try {
            return Regex.IsMatch(email,
                @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$",
                RegexOptions.None, TimeSpan.FromMilliseconds(250));
        } catch (RegexMatchTimeoutException) {
            // throw an exception explaining the task was failed 
            _ = email ?? throw new ArgumentException("email, Timeout/failed regexr processing.", nameof(email));
            return false;
        }
    }

    #endregion Helper methods
}
