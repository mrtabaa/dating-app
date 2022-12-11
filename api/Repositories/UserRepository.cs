namespace api.Repositories;
public class UserRepository : IUserRepository {

    #region Db and Token Settings
    const string _collectionName = "Users";
    private readonly IMongoCollection<AppUser> _collection;
    private readonly ITokenService _tokenService; // save user credential as a token
    private readonly CancellationToken _cancellationToken;

    // constructor - dependency injections
    public UserRepository(IMongoClient client, IMongoDbSettings dbSettings, ITokenService tokenService) {
        var dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<AppUser>(_collectionName);
// 
        _tokenService = tokenService;
        _cancellationToken = new CancellationToken();
    }
    #endregion

    #region CRUD
    public async Task<UserDto?> GetUser(string userId) {
        var user = await _collection.Find<AppUser>(user => user.Id == userId).FirstOrDefaultAsync(_cancellationToken);
        return user == null ? null : new UserDto (
            Id: user.Id,
            Email: user.Email
        );
    }

    public async Task<UpdateResult?> UpdateUser(string userId, UserRegisterDto userIn) {
        if (await CheckEmailExist(userIn))
            return null;

        using var hmac = new HMACSHA512();

        // if(_cancellationToken.IsCancellationRequested)
        //     _cancellationToken.ThrowIfCancellationRequested();

        var updatedUser = Builders<AppUser>.Update
        .Set(e => e.Email, userIn.Email)
        .Set(ph => ph.PasswordHash, hmac.ComputeHash(Encoding.UTF8.GetBytes(userIn.Password!)))
        .Set(ps => ps.PasswordSalt, hmac.Key);

        return await _collection.UpdateOneAsync<AppUser>(user => user.Id == userId, updatedUser, null, _cancellationToken);
    }

    public async Task<DeleteResult> DeleteUser(string userId) =>
        await _collection.DeleteOneAsync<AppUser>(user => user.Id == userId, _cancellationToken);

    #endregion CRUD

    #region Helper methods
    private async Task<bool> CheckEmailExist(UserRegisterDto userIn) {
        if (string.IsNullOrEmpty(userIn.Email))
            return false;

        return null != await _collection.Find<AppUser>(user => user.Email == userIn.Email).FirstOrDefaultAsync()
            ? true : false;
    }
    #endregion
}