namespace api.Repositories;
public class UserRepository : IUserRepository
{

    #region Db and Token Settings
    const string _collectionName = "users";
    private readonly IMongoCollection<AppUser> _collection;
    private readonly ILogger<UserRepository> _logger;
    private readonly IPhotoService _photoService;

    // constructor - dependency injections
    public UserRepository(
        IMongoClient client, IMongoDbSettings dbSettings, ILogger<UserRepository> logger, IPhotoService photoService)
    {
        var dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<AppUser>(_collectionName);

        _photoService = photoService;

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

    public async Task<MemberDto?> GetUserById(string? userId, CancellationToken cancellationToken)
    {
        if (userId is not null)
        {
            AppUser user = await _collection.Find<AppUser>(user => user.Id == userId).FirstOrDefaultAsync(cancellationToken);

            return user is null ? null : Mappers.GenerateMemberDto(user);
        }

        return null;
    }

    public async Task<MemberDto?> GetUserByEmail(string email, CancellationToken cancellationToken)
    {
        AppUser user = await _collection.Find<AppUser>(user => user.Email == email.ToLower().Trim()).FirstOrDefaultAsync(cancellationToken);

        return user is null ? null : Mappers.GenerateMemberDto(user);
    }

    public async Task<UpdateResult?> UpdateUser(MemberUpdateDto memberUpdateDto, string? userId, CancellationToken cancellationToken)
    {
        if (userId is not null)
        {
            var updatedUser = Builders<AppUser>.Update
            .Set(user => user.Schema, AppVariablesExtensions.AppVersions.Last<string>())
            .Set(user => user.Introduction, memberUpdateDto.Introduction.Trim())
            .Set(user => user.LookingFor, memberUpdateDto.LookingFor.Trim())
            .Set(user => user.Interests, memberUpdateDto.Interests.Trim())
            .Set(user => user.City, memberUpdateDto.City.Trim())
            .Set(user => user.Country, memberUpdateDto.Country.Trim());

            return await _collection.UpdateOneAsync<AppUser>(user => user.Id == userId, updatedUser, null, cancellationToken);
        }

        return null;
    }

    public async Task<DeleteResult?> DeleteUser(string userId, CancellationToken cancellationToken) =>
        await _collection.DeleteOneAsync<AppUser>(user => user.Id == userId, cancellationToken);

    public async Task<UpdateResult?> UploadPhotos(IEnumerable<IFormFile> files, string? userId, CancellationToken cancellationToken)
    {
        if (userId is null)
        {
            _logger.LogError("userId is Null");
            return null;
        }

        var user = await GetUserById(userId, cancellationToken);
        if (user is null)
        {
            _logger.LogError("user is Null / not found");
            return null;
        }

        // save file in Storage using PhotoService / userId makes the folder name
        IEnumerable<Photo>? addedPhotos = await _photoService.AddPhotos(files, userId, user.Photos);

        var updatedUser = Builders<AppUser>.Update
        .Set(user => user.Schema, AppVariablesExtensions.AppVersions.Last<string>())
        .Set(doc => doc.Photos, addedPhotos);

        return await _collection.UpdateOneAsync<AppUser>(user => user.Id == userId, updatedUser, null, cancellationToken);
    }

    public async Task<UpdateResult?> DeleteOnePhoto(string? userId, string? urlIn, CancellationToken cancellationToken)
    {
        var update = Builders<AppUser>.Update.PullFilter(user => user.Photos, photo => photo.Url_128 == urlIn);
        
        return await _collection.UpdateOneAsync<AppUser>(user => user.Id == userId, update, null, cancellationToken);
    }

    #endregion CRUD
}