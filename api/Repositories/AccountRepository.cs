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
    public async Task<LoginSuccessDto?> Create(UserRegisterDto userIn) {
        if (await CheckEmailExist(userIn!))
            return null;

        // prevent ComputeHash exception
        using var hmac = new HMACSHA512();
        var user = new AppUser {
            Schema = 0,
            Email = userIn.Email,
            Power = 0,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userIn.Password!)),
            PasswordSalt = hmac.Key,
            Verified = false,
            Name = userIn.Name,
            PhotoUrls = userIn.PhotoUrls,
            ProfilePhotoUrl = userIn.ProfilePhotoUrl,
        };

        // if insertion successful OR throw an exception
        try {
            await _collection!.InsertOneAsync(user); // mark ! after _collection! tells compiler it's NOT null

            return new LoginSuccessDto {
                Verified = user.Verified,
                PhotoUrls = user.PhotoUrls,
                Token = _tokenService.CreateToken(user)
            };
        } catch (System.Exception error) {
            throw error;
        }
    }

    public async Task<LoginSuccessDto?> Login(LoginDto userIn) {
        var user = await _collection.Find<AppUser>(user => user.Email == userIn.Email).FirstOrDefaultAsync(_cancellationToken);

        if (user == null)
            return null;

        using var hmac = new HMACSHA512(user.PasswordSalt!);
        var ComputeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userIn.Password!));
        if (user.PasswordHash!.SequenceEqual(ComputeHash))
            return new LoginSuccessDto {
                Verified = user.Verified,
                PhotoUrls = user.PhotoUrls,
                Token = _tokenService.CreateToken(user)
            };

        return null;
    }
    #endregion CRUD

    #region Helper methods
    private async Task<bool> CheckEmailExist(UserRegisterDto userIn) {
        if (string.IsNullOrEmpty(userIn.Email))
            return false;

        return null != await _collection.Find<AppUser>(user => user.Email == userIn.Email).FirstOrDefaultAsync()
            ? true : false;
    }
    #endregion Helper methods
}
