namespace api.Interfaces;

public interface IMemberRepository
{
    public Task<PagedList<AppUser>?> GetAllAsync(MemberParams userParams, CancellationToken cancellationToken);

    public Task<MemberDto?> GetByIdAsync(ObjectId userId, ObjectId? memberId, CancellationToken cancellationToken);

    public Task<MemberDto?> GetByUserNameAsync(ObjectId userId, string userName, CancellationToken cancellationToken);
}
