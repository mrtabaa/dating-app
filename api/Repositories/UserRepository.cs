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

    #region User Management
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

    public async Task<DeleteResult?> DeleteUser(string? userId, CancellationToken cancellationToken) =>
        await _collection.DeleteOneAsync<AppUser>(user => user.Id == userId, cancellationToken);
    #endregion User Management

    #region Photo Management
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
        IEnumerable<Photo>? addedPhotos = await _photoService.AddPhotosToDisk(files, userId, user.Photos);

        var updatedUser = Builders<AppUser>.Update
        .Set(user => user.Schema, AppVariablesExtensions.AppVersions.Last<string>())
        .Set(doc => doc.Photos, addedPhotos);

        return await _collection.UpdateOneAsync<AppUser>(user => user.Id == userId, updatedUser, null, cancellationToken);
    }

    public async Task<UpdateResult?> DeleteOnePhoto(string? userId, string? url_128_In, CancellationToken cancellationToken)
    {
        List<string> photoUrls = new();

        List<Photo>? photos = await _collection.AsQueryable().Where<AppUser>(user => user.Id == userId).Select(elem => elem.Photos).SingleOrDefaultAsync();

        if (photos is null || !photos.Any())
        {
            _logger.LogError("Album is empty OR the requested photo is the MainPhoto. No photo to delete.");
            return null;
        }

        foreach (Photo photo in photos)
        {
            if (photo.Url_128 == url_128_In)
            {
                if (photo.IsMain is true) // Prevent Main photo from deletion
                {
                    _logger.LogError("Main photo cannot be deleted!");
                    return null;
                }

                photoUrls.Add(photo.Url_128);
                photoUrls.Add(photo.Url_512);
                photoUrls.Add(photo.Url_1024);
            }
        }

        bool isDeleteSuccess = _photoService.DeletePhotoFromDisk(photoUrls);
        if (!isDeleteSuccess)
            _logger.LogError("Delete from disk failed. e.g. No photo found by this filePath.");

        var update = Builders<AppUser>.Update.PullFilter(user => user.Photos, photo => photo.Url_128 == url_128_In);

        return await _collection.UpdateOneAsync<AppUser>(user => user.Id == userId, update, null, cancellationToken);
    }

    public async Task<UpdateResult?> SetMainPhoto(string? userId, string photoUrlIn, CancellationToken cancellationToken)
    {
        // UNSET the previous main photo: Find the photo with IsMain True; update IsMain to False
        var filterOld = Builders<AppUser>.Filter.Where(user => user.Id == userId
                                                    && user.Photos.Any<Photo>(photo => photo.IsMain == true));
        var updateOld = Builders<AppUser>.Update.Set(user => user.Photos.FirstMatchingElement().IsMain, false);
        await _collection.UpdateOneAsync(filterOld, updateOld, null, cancellationToken);

        // SET the new main photo: find new photo by its Url_128; update IsMain to True
        var filterNew = Builders<AppUser>.Filter.Where(user => user.Id == userId
                                                    && user.Photos.Any<Photo>(photo => photo.Url_128 == photoUrlIn));
        var updateNew = Builders<AppUser>.Update.Set(user => user.Photos.FirstMatchingElement().IsMain, true);
        return await _collection.UpdateOneAsync(filterNew, updateNew, null, cancellationToken);
    }
    #endregion Photo Management

    #endregion CRUD
}