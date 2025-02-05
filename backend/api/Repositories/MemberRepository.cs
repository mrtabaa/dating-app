namespace api.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly IMongoCollection<AppUser>? _collection;
    private readonly IFollowRepository _followRepository;
    private readonly IPhotoService _photoService;

    #region Db and Token Settings

    // constructor - dependency injections
    public MemberRepository(IMongoClient client, IMyMongoDbSettings dbSettings, IPhotoService photoService, IFollowRepository followRepository)
    {
        IMongoDatabase dbName = client.GetDatabase(dbSettings.DatabaseName) ?? throw new ArgumentNullException(nameof(dbName));
        _collection = dbName.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);
        _photoService = photoService;
        _followRepository = followRepository;
    }

    #endregion

    #region Helpers

    private IMongoQueryable<AppUser> CreateQuery(MemberParams memberParams)
    {
        // calculate DOB based on user's selected Age
        DateOnly minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-memberParams.MaxAge - 1));
        DateOnly maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-memberParams.MinAge));

        // set the query to AsQueryable to use it against MongoDB later
        IMongoQueryable<AppUser> query = _collection.AsQueryable();

        query = query.Where(appUser => appUser.Id != memberParams.UserId); // Don't request/show the currentUser in the list
        query = query.Where(appUser => appUser.EmailConfirmed);
        query = query.Where(appUser => !(appUser.NormalizedUserName == "ADMIN" || appUser.UserName == "MODERATOR")); // Don't show admin/moderator
        query = query.Where(appUser => !(string.IsNullOrEmpty(appUser.NormalizedUserName) || appUser.NormalizedUserName.StartsWith("DEMO"))); // don't show demo users
        if (!string.IsNullOrEmpty(memberParams.UserNameOrKnownAs))
        {
            // Contains either UserName or KnowAs exists
            query = query.Where(appUser =>
                (!string.IsNullOrEmpty(appUser.NormalizedUserName)
                 && appUser.NormalizedUserName.Contains(memberParams.UserNameOrKnownAs.ToUpper())
                )
                || appUser.KnownAs.ToUpper().Contains(memberParams.UserNameOrKnownAs.ToUpper()));
        }

        if (!string.IsNullOrEmpty(memberParams.Gender))
            query = query.Where(appUser => appUser.Gender == memberParams.Gender);

        query = query.Where(appUser => appUser.DateOfBirth >= minDob && appUser.DateOfBirth <= maxDob); // Get ages between 2 age inputs
        query = memberParams.OrderBy switch
        {
            // sort users based on Age, Created or LastActive
            "age" => query.OrderByDescending(appUser => appUser.DateOfBirth).ThenBy(appUser => appUser.Id),
            "created" => query.OrderByDescending(appUser => appUser.CreatedOn).ThenBy(appUser => appUser.Id),
            _ => query.OrderByDescending(appUser => appUser.LastActive).ThenBy(appUser => appUser.Id)
        };

        return query;
    }

    private AppUser? ConvertAppUserPhotosToBlobPhotos(AppUser appUser)
    {
        List<Photo>? blobConvertedPhotos = _photoService.ConvertAllPhotosToBlobLinkWithSas(appUser.Photos)?.ToList();
        if (blobConvertedPhotos is null)
            return null;

        appUser.Photos = blobConvertedPhotos;

        return appUser;
    }

    #endregion Helpers

    #region CRUD

    public async Task<PagedList<AppUser>?> GetPagedListAsync(MemberParams memberParams, CancellationToken cancellationToken)
    {
        // For small lists
        // var appUsers = await _collection.Find<AppUser>(new BsonDocument()).ToListAsync(cancellationToken);

        PagedList<AppUser> appUsers = await PagedList<AppUser>.CreatePagedListAsync(
            CreateQuery(memberParams), memberParams.PageNumber, memberParams.PageSize, cancellationToken);

        #region Convert all members' appUser.Photos to BlobLinkFormat

        for (var i = 0; i < appUsers.Count; i++)
        {
            if (appUsers[i].Photos.Count <= 0) continue; // continue only if appUser has a photo

            AppUser? appUser = ConvertAppUserPhotosToBlobPhotos(appUsers[i]);

            if (appUser is null) return null;

            appUsers[i] = appUser;
        }

        #endregion Convert all members' appUser.Photos to BlobLinkFormat

        return appUsers;
    }

    public async Task<MemberDto?> GetByIdAsync(ObjectId userId, ObjectId? memberId, CancellationToken cancellationToken)
    {
        AppUser? appUser = await _collection.Find(appUser => appUser.Id == memberId).FirstOrDefaultAsync(cancellationToken);
        if (appUser is null)
            return null;

        appUser = ConvertAppUserPhotosToBlobPhotos(appUser);

        return appUser is null
            ? null
            : await _followRepository.CheckIsFollowing(userId, appUser, cancellationToken)
                ? Mappers.ConvertAppUserToMemberDto(appUser, true)
                : Mappers.ConvertAppUserToMemberDto(appUser);
    }

    public async Task<IEnumerable<AppUser>> GetAppUsersByIdsAsync(IEnumerable<ObjectId> membersIds, CancellationToken cancellationToken) =>
        await _collection.AsQueryable().Where(appUser => membersIds.Contains(appUser.Id)).ToListAsync(cancellationToken);

    public async Task<MemberDto?> GetByUserNameAsync(ObjectId userId, string userName, CancellationToken cancellationToken)
    {
        AppUser? appUser = await _collection.Find(appUser =>
            appUser.NormalizedUserName == userName.ToUpper().Trim() &&
            appUser.EmailConfirmed
        ).FirstOrDefaultAsync(cancellationToken);
        if (appUser is null)
            return null;
        
        appUser = ConvertAppUserPhotosToBlobPhotos(appUser);

        return appUser is null
            ? null
            : await _followRepository.CheckIsFollowing(userId, appUser, cancellationToken)
                ? Mappers.ConvertAppUserToMemberDto(appUser, true)
                : Mappers.ConvertAppUserToMemberDto(appUser);
    }

    #endregion CRUD
}