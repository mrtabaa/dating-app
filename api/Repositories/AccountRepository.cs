using System.Text.RegularExpressions;

namespace api.Repositories;

public class AccountRepository : IAccountRepository
{

    #region Db and Token Settings
    const string _collectionName = "Users";
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

        // email format validation
        if (!validateEmailPattern(userInput.Email))
            return new LoginSuccessDto(
                Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                Token: null,
                Name: null,
                Email: null,
                BadEmailPattern: true);

        if (await CheckEmailExist(userInput.Email.ToLower()))
            return null;

        AppUser appUser = Mappers.AppUser(userInput);

        // if insertion successful OR throw an exception
        await _collection!.InsertOneAsync(appUser); // mark ! after _collection! tells compiler it's nullable

        return new LoginSuccessDto(
            Schema: AppVariablesExtensions.AppVersions.Last<string>(),
            Token: _tokenService.CreateToken(appUser),
            Name: appUser.Name,
            Email: appUser.Email,
            BadEmailPattern: false
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
                Email: user.Email,
                BadEmailPattern: false
            );

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
    /// * Not case-sensitive
    /// * Stops operation if takes longer than 250ms and throw a detailed exception
    /// </summary>
    /// <param name="email"></param>
    /// <returns>success: true | fail: false </returns>
    /// <exception cref="ArgumentException"></exception>
    private bool validateEmailPattern(string email)
    {
        try
        {
            return Regex.IsMatch(email,
                @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$",
                RegexOptions.None, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            // throw an exception explaining the task was failed 
            _ = email ?? throw new ArgumentException("email, Timeout regexr processing.", nameof(email));
            return false;
        }
    }

    #endregion Helper methods
}
