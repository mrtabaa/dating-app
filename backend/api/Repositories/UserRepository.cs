namespace api.Repositories;
public class UserRepository : IUserRepository
{
    #region Db and Token Settings
    private readonly IMongoCollection<AppUser> _collection;
    private readonly ILogger<UserRepository> _logger;
    private readonly IPhotoService _photoService;

    // constructor - dependency injections
    public UserRepository(
        IMongoClient client, IMongoDbSettings dbSettings,
        ILogger<UserRepository> logger, IPhotoService photoService
        )
    {
        var dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<AppUser>(AppVariablesExtensions.collectionUsers);

        _photoService = photoService;

        _logger = logger;
    }
    #endregion

    #region CRUD

    #region User Management
    public async Task<AppUser?> GetByIdAsync(ObjectId userId, CancellationToken cancellationToken)
    {
        return await _collection.Find<AppUser>(appUser => appUser.Id == userId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AppUser?> GetByEmailAsync(string userEmail, CancellationToken cancellationToken)
    {
        AppUser appUser = await _collection.Find<AppUser>(appUser => appUser.Email == userEmail).FirstOrDefaultAsync(cancellationToken);

        return appUser is null ? null : appUser;
    }

    public async Task<ObjectId?> GetIdByEmailAsync(string userEmail, CancellationToken cancellationToken)
    {
        return await _collection.AsQueryable<AppUser>()
            .Where(appUser => appUser.Email == userEmail)
            .Select(appUser => appUser.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<string?> GetKnownAsByEmailAsync(string userEmail, CancellationToken cancellationToken)
    {
        return await _collection.AsQueryable<AppUser>()
            .Where(appUser => appUser.Email == userEmail)
            .Select(appUser => appUser.KnownAs)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UpdateResult?> UpdateUserAsync(UserUpdateDto userUpdateDto, string? userEmail, CancellationToken cancellationToken)
    {
        if (userEmail is null)
            return null;

        var updatedUser = Builders<AppUser>.Update
        .Set(appUser => appUser.Schema, AppVariablesExtensions.AppVersions.Last<string>())
        .Set(appUser => appUser.Introduction, userUpdateDto.Introduction.Trim())
        .Set(appUser => appUser.LookingFor, userUpdateDto.LookingFor.Trim())
        .Set(appUser => appUser.Interests, userUpdateDto.Interests.Trim())
        .Set(appUser => appUser.City, userUpdateDto.City.Trim())
        .Set(appUser => appUser.Country, userUpdateDto.Country.Trim());

        return await _collection.UpdateOneAsync<AppUser>(appUser => appUser.Email == userEmail, updatedUser, null, cancellationToken);
    }

    public async Task<UpdateResult?> UpdateLastActive(string loggedInUserEmail, CancellationToken cancellationToken)
    {
        UpdateDefinition<AppUser> updatedUserLastActive = Builders<AppUser>.Update
            .Set(appUser => appUser.LastActive, DateTime.UtcNow);

        return await _collection.UpdateOneAsync<AppUser>(appUser => appUser.Email == loggedInUserEmail, updatedUserLastActive, null, cancellationToken);
    }
    #endregion User Management

    #region Photo Management
    public async Task<Photo?> UploadPhotoAsync(IFormFile file, string? userEmail, CancellationToken cancellationToken)
    {
        if (userEmail is null)
        {
            _logger.LogError("userEmail is Null");
            return null;
        }

        AppUser? appUser = await GetByEmailAsync(userEmail, cancellationToken);
        if (appUser is null)
        {
            _logger.LogError("user is Null / not found");
            return null;
        }

        // save file in Storage using PhotoService / userEmail makes the folder name
        string[]? photoUrls = await _photoService.AddPhotoToDisk(file, appUser.Id.ToString());

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

            UpdateResult result = await _collection.UpdateOneAsync<AppUser>(appUser => appUser.Email == userEmail, updatedUser, null, cancellationToken);

            // return the save photo if save on disk and DB
            return photoUrls is not null && result.ModifiedCount == 1 ? photo : null;
        }

        _logger.LogError("PhotoService saving photo to disk failed.");
        return null;
    }

    public async Task<UpdateResult?> SetMainPhotoAsync(string? userEmail, string photoUrlIn, CancellationToken cancellationToken)
    {
        #region  UNSET the previous main photo: Find the photo with IsMain True; update IsMain to False
        // set query
        FilterDefinition<AppUser> filterOld = Builders<AppUser>.Filter
            .Where(appUser =>
                appUser.Email == userEmail && appUser.Photos.Any<Photo>(photo => photo.IsMain == true));

        UpdateDefinition<AppUser> updateOld = Builders<AppUser>.Update
            .Set(appUser => appUser.Photos.FirstMatchingElement().IsMain, false);

        await _collection.UpdateOneAsync(filterOld, updateOld, null, cancellationToken);
        #endregion

        #region  SET the new main photo: find new photo by its Url_128; update IsMain to True
        FilterDefinition<AppUser> filterNew = Builders<AppUser>.Filter
            .Where(appUser =>
                appUser.Email == userEmail && appUser.Photos.Any<Photo>(photo => photo.Url_165 == photoUrlIn));

        UpdateDefinition<AppUser> updateNew = Builders<AppUser>.Update
            .Set(appUser => appUser.Photos.FirstMatchingElement().IsMain, true);
        #endregion

        return await _collection.UpdateOneAsync(filterNew, updateNew, null, cancellationToken);
    }

    public async Task<UpdateResult?> DeleteOnePhotoAsync(string? userEmail, string? url_165_In, CancellationToken cancellationToken)
    {
        // Find the photo in AppUser
        Photo photo = await _collection.AsQueryable()
            .Where(appUser => appUser.Email == userEmail) // filter by user email
            .SelectMany(appUser => appUser.Photos) // flatten the Photos array
            .Where(photo => photo.Url_165 == url_165_In) // filter by photo url
            .FirstOrDefaultAsync(cancellationToken); // return the photo or null

        if (photo is null)
            return null;

        bool isDeleteSuccess = await _photoService.DeletePhotoFromDisk(photo);
        if (!isDeleteSuccess)
            _logger.LogError("Delete from disk failed. See the logs.");

        var update = Builders<AppUser>.Update.PullFilter(appUser => appUser.Photos, photo => photo.Url_165 == url_165_In);

        return await _collection.UpdateOneAsync<AppUser>(appUser => appUser.Email == userEmail, update, null, cancellationToken);
    }
    #endregion Photo Management

    #endregion CRUD
}