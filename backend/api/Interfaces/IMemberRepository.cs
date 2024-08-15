namespace api.Interfaces;

public interface IMemberRepository
{
    public Task<PagedList<AppUser>?> GetPagedListAsync(MemberParams userParams, CancellationToken cancellationToken);

    public Task<MemberDto?> GetByIdAsync(ObjectId userId, ObjectId? memberId, CancellationToken cancellationToken);

    public Task<IEnumerable<AppUser>> GetAppUsersByIdsAsync(IEnumerable<ObjectId> membersIds, CancellationToken cancellationToken);

    public Task<MemberDto?> GetByUserNameAsync(ObjectId userId, string userName, CancellationToken cancellationToken);
}
