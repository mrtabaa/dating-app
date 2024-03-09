namespace api.Interfaces;

public interface IMemberRepository
{
    public Task<PagedList<AppUser>> GetAllAsync(MemberParams userParams, CancellationToken cancellationToken);

    public Task<MemberDto?> GetByIdAsync(ObjectId? memberId, CancellationToken cancellationToken);

    public Task<MemberDto?> GetByUserNameAsync(string userName, CancellationToken cancellationToken);
}
