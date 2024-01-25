namespace api.Interfaces;

public interface IMemberRepository
{
    public Task<PagedList<AppUser>> GetMembersAsync(UserParams userParams, CancellationToken cancellationToken);

    public Task<MemberDto?> GetMemberByIdAsync(string? memberId, CancellationToken cancellationToken);
    
    public Task<MemberDto?> GetMemberByEmailAsync(string email, CancellationToken cancellationToken);
}
