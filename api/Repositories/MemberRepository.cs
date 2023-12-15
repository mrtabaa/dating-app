namespace api.Repositories;

public class MemberRepository : IMemberRepository
{
    #region Db and Token Settings
    const string _collectionName = "users";
    private readonly IMongoCollection<AppUser> _collection;

    // constructor - dependency injections
    public MemberRepository(IMongoClient client, IMongoDbSettings dbSettings)
    {
        var dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<AppUser>(_collectionName);
    }
    #endregion

    #region CRUD
    public async Task<PagedList<AppUser>> GetMembersAsync(UserParams memberParams, CancellationToken cancellationToken)
    {
        // For small lists
        // var appUsers = await _collection.Find<AppUser>(new BsonDocument()).ToListAsync(cancellationToken);

        // calculate DOB based on user's selected Age
        var MinDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-memberParams.MaxAge - 1));
        var MaxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-memberParams.MinAge));

        // set query to AsQuerable to use it agains MongoDB in another file e.g. PagedList
        IMongoQueryable<AppUser> query = _collection.AsQueryable().Where<AppUser>(user =>
                user.Id != memberParams.CurrentUserId // don't request/show the currentUser in the list
                && user.Gender != memberParams.Gender // get the opposite gender only
                && user.DateOfBirth >= MinDob && user.DateOfBirth <= MaxDob
            );

        PagedList<AppUser> pagedAppUsers = await PagedList<AppUser>.CreatePagedListAsync(query, memberParams.PageNumber, memberParams.PageSize, cancellationToken);

        return pagedAppUsers;
    }

    public async Task<MemberDto?> GetMemberByIdAsync(string? memberId, CancellationToken cancellationToken)
    {
        AppUser appUser = await _collection.Find<AppUser>(appUser => appUser.Id == memberId).FirstOrDefaultAsync(cancellationToken);

        return appUser is null ? null : Mappers.ConvertAppUserToMemberDto(appUser);
    }

    public async Task<MemberDto?> GetMemberByEmailAsync(string email, CancellationToken cancellationToken)
    {
        AppUser appUser = await _collection.Find<AppUser>(appUser => appUser.Email == email.ToLower().Trim()).FirstOrDefaultAsync(cancellationToken);

        return appUser is null ? null : Mappers.ConvertAppUserToMemberDto(appUser);
    }
    #endregion CRUD
}
