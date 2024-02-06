namespace api.Interfaces;

public interface IMemberRepository
{
    public Task<PagedList<AppUser>> GetMembersAsync(MemberParams userParams, CancellationToken cancellationToken);

    public Task<MemberDto?> GetMemberByIdAsync(ObjectId? memberId, CancellationToken cancellationToken);

    public Task<MemberDto?> GetMemberByEmailAsync(string email, CancellationToken cancellationToken);
}
