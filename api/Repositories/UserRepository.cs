namespace api.Repositories;
public class UserRepository : IUserRepository
{

    #region Db and Token Settings
    const string _collectionName = "Users";
    private readonly IMongoCollection<AppUser> _collection;
    private readonly ILogger<UserRepository> _logger;

    // constructor - dependency injections
    public UserRepository(
        IMongoClient client, IMongoDbSettings dbSettings, ILogger<UserRepository> logger)
    {

        var dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<AppUser>(_collectionName);

        _logger = logger;
    }
    #endregion

    #region CRUD
    public async Task<List<MemberDto?>> GetUsers(CancellationToken cancellationToken)
    {
        List<MemberDto?> memberDtos = new();

        // For small lists
        // var appUsers = await _collection.Find<AppUser>(new BsonDocument()).ToListAsync(cancellationToken);

        // For large lists
        await _collection.Find<AppUser>(new BsonDocument())
        .ForEachAsync(appUser =>
        {
            memberDtos.Add(Mappers.GenerateMemberDto(appUser));
        }, cancellationToken);

        _logger.LogError("GetUsers was canceled.");

        return memberDtos;
    }

    public async Task<MemberDto?> GetUserById(string userId, CancellationToken cancellationToken)
    {
        AppUser user = await _collection.Find<AppUser>(user => user.Id == userId).FirstOrDefaultAsync(cancellationToken);

        return user == null ? null : Mappers.GenerateMemberDto(user);
    }

    public async Task<MemberDto?> GetUserByEmail(string email, CancellationToken cancellationToken)
    {
        AppUser user = await _collection.Find<AppUser>(user => user.Email == email).FirstOrDefaultAsync(cancellationToken);

        return user == null ? null : Mappers.GenerateMemberDto(user);
    }

    public async Task<UpdateResult?> UpdateUser(MemberUpdateDto memberUpdateDto, string userId, CancellationToken cancellationToken)
    {
        var updatedUser = Builders<AppUser>.Update
        .Set(user => user.Schema, AppVariablesExtensions.AppVersions.Last<string>())
        .Set(user => user.Introduction, memberUpdateDto.Introduction)
        .Set(user => user.LookingFor, memberUpdateDto.LookingFor)
        .Set(user => user.Interests, memberUpdateDto.Interests)
        .Set(user => user.City, memberUpdateDto.City)
        .Set(user => user.Country, memberUpdateDto.Country);

        return await _collection.UpdateOneAsync<AppUser>(user => user.Id == userId, updatedUser, null, cancellationToken);
    }

    public async Task<DeleteResult?> DeleteUser(string userId, CancellationToken cancellationToken) =>
        await _collection.DeleteOneAsync<AppUser>(user => user.Id == userId, cancellationToken);

    #endregion CRUD
}