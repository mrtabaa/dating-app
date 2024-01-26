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
    public async Task<AppUser?> GetByIdAsync(string? userId, CancellationToken cancellationToken)
    {
        if (userId is not null)
        {
            AppUser appUser = await _collection.Find<AppUser>(appUser => appUser.Id == userId).FirstOrDefaultAsync(cancellationToken);

            return appUser is null ? null : appUser;
        }

        return null;
    }

    public async Task<UpdateResult?> UpdateUserAsync(UserUpdateDto userUpdateDto, string? userId, CancellationToken cancellationToken)
    {
        if (userId is not null)
        {
            var updatedUser = Builders<AppUser>.Update
            .Set(appUser => appUser.Schema, AppVariablesExtensions.AppVersions.Last<string>())
            .Set(appUser => appUser.Introduction, userUpdateDto.Introduction.Trim())
            .Set(appUser => appUser.LookingFor, userUpdateDto.LookingFor.Trim())
            .Set(appUser => appUser.Interests, userUpdateDto.Interests.Trim())
            .Set(appUser => appUser.City, userUpdateDto.City.Trim())
            .Set(appUser => appUser.Country, userUpdateDto.Country.Trim());

            return await _collection.UpdateOneAsync<AppUser>(appUser => appUser.Id == userId, updatedUser, null, cancellationToken);
        }

        return null;
    }

    public async Task<UpdateResult?> UpdateLastActive(string loggedInUserId, CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser> updatedUserLastActive = Builders<AppUser>.Update
            .Set(appUser => appUser.LastActive, DateTime.UtcNow);

        return await _collection.UpdateOneAsync<AppUser>(appUser => appUser.Id == loggedInUserId, updatedUserLastActive, null, cancellationToken);
    }

    public async Task<DeleteResult?> DeleteUserAsync(string? userId, CancellationToken cancellationToken) =>
        await _collection.DeleteOneAsync<AppUser>(appUser => appUser.Id == userId, cancellationToken);
    #endregion User Management

    #region Photo Management
    public async Task<Photo?> UploadPhotoAsync(IFormFile file, string? userId, CancellationToken cancellationToken)
    {
        if (userId is null)
        {
            _logger.LogError("userId is Null");
            return null;
        }

        AppUser? appUser = await GetByIdAsync(userId, cancellationToken);
        if (appUser is null)
        {
            _logger.LogError("user is Null / not found");
            return null;
        }

        // save file in Storage using PhotoService / userId makes the folder name
        string[]? photoUrls = await _photoService.AddPhotoToDisk(file, userId);

        if (photoUrls is not null)
        {
            Photo photo;
            if (appUser.Photos.Count == 0) // if user's album is empty set IsMain: true
            {
                photo = Mappers.ConvertPhotoUrlsToPhoto(photoUrls, isMain: true);
            }
            else // user's album is not empty
            {
                photo = Mappers.ConvertPhotoUrlsToPhoto(photoUrls, isMain: false);
            }

            // save to DB
            appUser.Photos.Add(photo);

            var updatedUser = Builders<AppUser>.Update
                .Set(appUser => appUser.Schema, AppVariablesExtensions.AppVersions.Last<string>())
                .Set(doc => doc.Photos, appUser.Photos);

            UpdateResult result = await _collection.UpdateOneAsync<AppUser>(appUser => appUser.Id == userId, updatedUser, null, cancellationToken);

            // return the save photo if save on disk and DB
            return photoUrls is not null && result.ModifiedCount == 1 ? photo : null;
        }

        _logger.LogError("PhotoService saving photo to disk failed.");
        return null;
    }

    public async Task<UpdateResult?> DeleteOnePhotoAsync(string? userId, string? url_165_In, CancellationToken cancellationToken)
    {
        List<string> photoUrls = [];

        List<Photo>? photos = await _collection.AsQueryable().Where<AppUser>(appUser => appUser.Id == userId).Select(elem => elem.Photos).SingleOrDefaultAsync();

        if (photos is null || photos.Count() < 2)
        {
            _logger.LogError("Album is empty OR the requested photo is the MainPhoto. No photo to delete.");
            return null;
        }

        foreach (Photo photo in photos)
        {
            if (photo.Url_165 == url_165_In)
            {
                if (photo.IsMain is true) // Prevent Main photo from deletion
                {
                    _logger.LogError("Main photo cannot be deleted!");
                    return null;
                }

                photoUrls.Add(photo.Url_165);
                photoUrls.Add(photo.Url_256);
                photoUrls.Add(photo.Url_enlarged);
            }
        }

        bool isDeleteSuccess = _photoService.DeletePhotoFromDisk(photoUrls);
        if (!isDeleteSuccess)
            _logger.LogError("Delete from disk failed. e.g. No photo found by this filePath.");

        var update = Builders<AppUser>.Update.PullFilter(appUser => appUser.Photos, photo => photo.Url_165 == url_165_In);

        return await _collection.UpdateOneAsync<AppUser>(appUser => appUser.Id == userId, update, null, cancellationToken);
    }

    public async Task<UpdateResult?> SetMainPhotoAsync(string? userId, string photoUrlIn, CancellationToken cancellationToken)
    {
        // UNSET the previous main photo: Find the photo with IsMain True; update IsMain to False
        var filterOld = Builders<AppUser>.Filter.Where(appUser => appUser.Id == userId
                                                    && appUser.Photos.Any<Photo>(photo => photo.IsMain == true));
        var updateOld = Builders<AppUser>.Update.Set(user => user.Photos.FirstMatchingElement().IsMain, false);
        await _collection.UpdateOneAsync(filterOld, updateOld, null, cancellationToken);

        // SET the new main photo: find new photo by its Url_128; update IsMain to True
        var filterNew = Builders<AppUser>.Filter.Where(appUser => appUser.Id == userId
                                                    && appUser.Photos.Any<Photo>(photo => photo.Url_165 == photoUrlIn));
        var updateNew = Builders<AppUser>.Update.Set(appUser => appUser.Photos.FirstMatchingElement().IsMain, true);
        return await _collection.UpdateOneAsync(filterNew, updateNew, null, cancellationToken);
    }
    #endregion Photo Management

    #endregion CRUD
}