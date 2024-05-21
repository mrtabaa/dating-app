namespace api.Repositories;
public class UserRepository : IUserRepository
{
    #region Db and Token Settings
    private readonly IMongoCollection<AppUser> _collection;
    private readonly ILogger<UserRepository> _logger;
    private readonly IPhotoService _photoService;
    private readonly ITokenService _tokenService;

    // constructor - dependency injections
    public UserRepository(
        IMongoClient client, IMyMongoDbSettings dbSettings,
        ILogger<UserRepository> logger, IPhotoService photoService, ITokenService tokenService
        )
    {
        var dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<AppUser>(AppVariablesExtensions.collectionUsers);

        _photoService = photoService;
        _tokenService = tokenService;
        _logger = logger;
    }
    #endregion

    #region CRUD

    #region User Management
    public async Task<AppUser?> GetByIdAsync(ObjectId userId, CancellationToken cancellationToken) =>
        await _collection.Find<AppUser>(appUser => appUser.Id == userId).FirstOrDefaultAsync(cancellationToken);

    public async Task<AppUser?> GetByHashedIdAsync(ObjectId userId, CancellationToken cancellationToken) =>
         await _collection.Find<AppUser>(appUser => appUser.Id == userId).FirstOrDefaultAsync(cancellationToken);

    public async Task<AppUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken) =>
      await _collection.Find<AppUser>(appUser => appUser.NormalizedUserName == userName.ToUpper().Trim()).FirstOrDefaultAsync(cancellationToken);

    public async Task<ObjectId?> GetIdByUserNameAsync(string userName, CancellationToken cancellationToken) =>
         await _collection.AsQueryable<AppUser>()
            .Where(appUser => appUser.NormalizedUserName == userName.ToUpper().Trim())
            .Select(appUser => appUser.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<string?> GetKnownAsByUserNameAsync(string userName, CancellationToken cancellationToken) =>
        await _collection.AsQueryable<AppUser>()
            .Where(appUser => appUser.NormalizedUserName == userName.ToUpper().Trim())
            .Select(appUser => appUser.KnownAs)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<string?> GetGenderByIdAsync(ObjectId userId, CancellationToken cancellationToken) =>
        await _collection.AsQueryable()
            .Where<AppUser>(appUser => appUser.Id == userId)
            .Select(appUser => appUser.Gender)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<UpdateResult?> UpdateUserAsync(UserUpdateDto userUpdateDto, ObjectId userId, CancellationToken cancellationToken)
    {
        var updatedUser = Builders<AppUser>.Update
        .Set(appUser => appUser.Schema, AppVariablesExtensions.AppVersions.Last<string>())
        .Set(appUser => appUser.KnownAs, userUpdateDto.KnownAs?.Trim())
        .Set(appUser => appUser.Introduction, userUpdateDto.Introduction?.Trim())
        .Set(appUser => appUser.LookingFor, userUpdateDto.LookingFor?.Trim())
        .Set(appUser => appUser.Interests, userUpdateDto.Interests?.Trim())
        .Set(appUser => appUser.Country, userUpdateDto.Country.Trim())
        .Set(appUser => appUser.City, userUpdateDto.City.Trim())
        .Set(appUser => appUser.IsProfileCompleted, true);

        return await _collection.UpdateOneAsync<AppUser>(appUser => appUser.Id == userId, updatedUser, null, cancellationToken);
    }

    #endregion User Management

    #region Photo Management
    public async Task<PhotoUploadStatus> UploadPhotoAsync(IFormFile file, ObjectId userId, CancellationToken cancellationToken)
    {
        PhotoUploadStatus photoUploadStatus = new();

        AppUser? appUser = await GetByIdAsync(userId, cancellationToken);
        if (appUser is null)
        {
            _logger.LogError("appUser is Null / not found");
            return photoUploadStatus;
        }

        if (appUser.Photos.Count >= photoUploadStatus.MaxPhotosLimit)
        {
            photoUploadStatus.IsMaxPhotoReached = true;
            return photoUploadStatus;
        }

        // save file in Storage using PhotoService / userEmail makes the folder name
        string[]? photoUrls = await _photoService.AddPhotoToBlob(file, appUser.Id.ToString(), cancellationToken);

        if (photoUrls is null)
            return photoUploadStatus;

        Photo? photo;
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

        // If db is not updated and cancellation is requested, delete the photo from Azure Blob Storage and stop proceeding.
        // Consider using "Full Optimistic Locking / appUser Version comparison" if data integrity is required. 
        if (result.ModifiedCount == 0 && cancellationToken.IsCancellationRequested)
        {
            await _photoService.DeletePhotoFromBlob(photo, CancellationToken.None);
            return photoUploadStatus;
        }

        photo = _photoService.ConvertPhotoToBlobLinkWithSas(photo);

        // return the saved photo if save on Azure and DB
        photoUploadStatus.Photo = photo;
        return photoUploadStatus;
    }

    public async Task<UpdateResult?> SetMainPhotoAsync(ObjectId userId, string blob_url_165_In, CancellationToken cancellationToken)
    {
        // Convert blobUri to dbUri
        string? dbUri = BlobUriAndDbUriExtension.ConvertBlobUriToDbUri(blob_url_165_In, containerName: "photos");
        if (string.IsNullOrEmpty(dbUri)) return null;

        #region  UNSET the previous main photo: Find the photo with IsMain True; update IsMain to False
        // set query
        FilterDefinition<AppUser> filterOld = Builders<AppUser>.Filter
            .Where(appUser =>
                appUser.Id == userId && appUser.Photos.Any<Photo>(photo => photo.IsMain == true));

        UpdateDefinition<AppUser> updateOld = Builders<AppUser>.Update
            .Set(appUser => appUser.Photos.FirstMatchingElement().IsMain, false);

        await _collection.UpdateOneAsync(filterOld, updateOld, null, cancellationToken);
        #endregion

        #region  SET the new main photo: find new photo by its Url_128; update IsMain to True
        FilterDefinition<AppUser> filterNew = Builders<AppUser>.Filter
            .Where(appUser =>
                appUser.Id == userId && appUser.Photos.Any<Photo>(photo => photo.Url_165 == dbUri));

        UpdateDefinition<AppUser> updateNew = Builders<AppUser>.Update
            .Set(appUser => appUser.Photos.FirstMatchingElement().IsMain, true);
        #endregion

        return await _collection.UpdateOneAsync(filterNew, updateNew, null, cancellationToken);
    }

    public async Task<PhotoDeleteResponse> DeletePhotoAsync(ObjectId userId, string? blob_url_165_In, CancellationToken cancellationToken)
    {
        PhotoDeleteResponse photoDeleteResponse = new();

        string? dbUri = BlobUriAndDbUriExtension.ConvertBlobUriToDbUri(blob_url_165_In, containerName: "photos");

        // Find the photo to delete
        Photo photo = await _collection.AsQueryable()
            .Where(appUser => appUser.Id == userId) // filter by user id
            .SelectMany(appUser => appUser.Photos) // flatten the Photos array
            .Where(photo => photo.Url_165 == dbUri) // filter by photo url
            .FirstOrDefaultAsync(cancellationToken); // return the photo or null

        if (photo is null)
        {
            photoDeleteResponse.IsDeletionFailed = true;
            return photoDeleteResponse;
        }

        // Create Filter of the matching Id
        FilterDefinition<AppUser> filterDef = Builders<AppUser>.Filter
            .Eq(appUser => appUser.Id, userId);

        // Make updateDef to Delete the photo
        UpdateDefinition<AppUser>? updateDef = Builders<AppUser>.Update
            .PullFilter(appUser => appUser.Photos, photo => photo.Url_165 == dbUri);

        // Execute updating the AppUser
        await _collection.UpdateOneAsync(filterDef, updateDef, null, cancellationToken);

        // SET the next photo to main (if any)
        if (photo.IsMain)
        {
            string? newMainBlob = await SetNextPhotoAsMain(filterDef, userId, cancellationToken);

            if (!string.IsNullOrEmpty(newMainBlob))
            {
                photoDeleteResponse.NewMainUrl = newMainBlob;
                return photoDeleteResponse;
            }

            photoDeleteResponse.NewMainUrl = newMainBlob;
            return photoDeleteResponse;
        }

        // Delete blob
        bool isDeleteSuccess = await _photoService.DeletePhotoFromBlob(photo, cancellationToken);
        if (!isDeleteSuccess)
        {
            _logger.LogError("Delete from disk failed. See the logs.");
            photoDeleteResponse.IsDeletionFailed = true;
            return photoDeleteResponse;
        }

        return photoDeleteResponse;
    }

    /// <summary>
    /// Get the filterDef which matches 'userId'. 
    /// If deleted photo was main, set the next photo as main and return the new blob url to send to the client.
    /// </summary>
    /// <param name="filterDef"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<string?> SetNextPhotoAsMain(FilterDefinition<AppUser> filterDef, ObjectId userId, CancellationToken cancellationToken)
    {
        // Add this filter to filterDef which already includes userId. Check if there's a photo with IsMain == false
        filterDef = Builders<AppUser>.Filter
            .Where(appUser => appUser.Photos.Any<Photo>(photo => photo.IsMain == false));

        // Create the updateDef
        UpdateDefinition<AppUser> updateDef = Builders<AppUser>.Update
            .Set(appUser => appUser.Photos.FirstMatchingElement().IsMain, true); // Find the next 'false' and make it 'true'

        UpdateResult updateResult = await _collection.UpdateOneAsync(filterDef, updateDef, null, cancellationToken);

        if (updateResult.ModifiedCount == 0) return null;

        // Do NOT used the previous filter. Will give error if checking IsMain == false. 
        AppUser? updatedAppUser = await _collection.Find<AppUser>(appUser => appUser.Id == userId).FirstOrDefaultAsync(cancellationToken);

        if (updatedAppUser is null)
        {
            const string message = "AppUser cannot be null at this point. See the exception-logs in db.";
            _logger.LogError(message);
            throw new InvalidOperationException(message);
        }

        // convert next photoUrl 
        string? blobPhotoUrl = _photoService.ConvertPhotoToBlobLinkWithSas(
            updatedAppUser.Photos.Where<Photo>(photo => photo.IsMain).FirstOrDefault())?.Url_165;

        return string.IsNullOrEmpty(blobPhotoUrl) ? null : blobPhotoUrl;
    }
    #endregion Photo Management

    #endregion CRUD

    #region Helpers

    #endregion
}