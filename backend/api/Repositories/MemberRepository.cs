namespace api.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly IMongoCollection<AppUser>? _collection;
    private readonly IPhotoService _photoService;
    #region Db and Token Settings

    // constructor - dependency injections
    public MemberRepository(IMongoClient client, IMyMongoDbSettings dbSettings, IPhotoService photoService)
    {
        // TODO make all dbName/s nullable and handle them with dbName?.GetCollection
        IMongoDatabase? dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName?.GetCollection<AppUser>(AppVariablesExtensions.collectionUsers);
        _photoService = photoService;
    }
    #endregion

    #region CRUD
    public async Task<PagedList<AppUser>?> GetAllAsync(MemberParams memberParams, CancellationToken cancellationToken)
    {
        // For small lists
        // var appUsers = await _collection.Find<AppUser>(new BsonDocument()).ToListAsync(cancellationToken);

        #region Filters
        // calculate DOB based on user's selected Age
        var MinDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-memberParams.MaxAge - 1));
        var MaxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-memberParams.MinAge));

        // set query to AsQuerable to use it agains MongoDB later
        IMongoQueryable<AppUser> query = _collection.AsQueryable();

        query = query.Where(appUser => appUser.Id != memberParams.UserId); // don't request/show the currentUser in the list
        query = query.Where(appUser => appUser.Gender == memberParams.Gender); // get the opposite gender by default. It's set in MemberController
        query = query.Where(appUser => appUser.DateOfBirth >= MinDob && appUser.DateOfBirth <= MaxDob); // get ages between 2 age inputs
        query = memberParams.OrderBy switch
        { // sort users based on Age, Created or LastActive
            "age" => query.OrderByDescending(appUser => appUser.DateOfBirth).ThenBy(appUser => appUser.Id),
            "created" => query.OrderByDescending(appUser => appUser.CreatedOn).ThenBy(appUser => appUser.Id),
            _ => query.OrderByDescending(appUser => appUser.LastActive).ThenBy(appUser => appUser.Id)
        };
        #endregion Filters

        PagedList<AppUser> appUsers = await PagedList<AppUser>.CreatePagedListAsync(query, memberParams.PageNumber, memberParams.PageSize, cancellationToken);

        #region Convert all members' appUser.Photos to BlobLinkFormat
        for (int i = 0; i < appUsers.Count; i++)
        {
            AppUser? appUser = ConvertAppUserPhotosToBlobPhotos(appUsers[i]);

            if(appUser is null) return null;

            appUsers[i] = appUser;
        }
        #endregion Convert all members' appUser.Photos to BlobLinkFormat

        return appUsers;
    }

    public async Task<MemberDto?> GetByIdAsync(ObjectId? memberId, CancellationToken cancellationToken)
    {
        AppUser? appUser = await _collection.Find<AppUser>(appUser => appUser.Id == memberId).FirstOrDefaultAsync(cancellationToken);

        appUser = ConvertAppUserPhotosToBlobPhotos(appUser);

        return appUser is null ? null : Mappers.ConvertAppUserToMemberDto(appUser);
    }

    public async Task<MemberDto?> GetByUserNameAsync(string userName, CancellationToken cancellationToken)
    {
        AppUser? appUser = await _collection.Find<AppUser>(appUser => appUser.NormalizedUserName == userName.ToUpper().Trim()).FirstOrDefaultAsync(cancellationToken);

        appUser = ConvertAppUserPhotosToBlobPhotos(appUser);

        return appUser is null ? null : Mappers.ConvertAppUserToMemberDto(appUser);
    }
    #endregion CRUD

    private AppUser? ConvertAppUserPhotosToBlobPhotos(AppUser appUser)
    {
        List<Photo>? blobConvertedPhotos = _photoService.ConvertAllPhotosToBlobLinkFormat(appUser.Photos)?.ToList();
        if (blobConvertedPhotos is null)
            return null;

        appUser.Photos = blobConvertedPhotos;

        return appUser;
    }
}
