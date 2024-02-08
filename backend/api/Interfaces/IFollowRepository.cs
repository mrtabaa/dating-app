namespace api.Interfaces;

public interface IFollowRepository
{
    public Task<FolowStatus> AddFollowAsync(string? loggedInUserEmail, string targetMemberEmail, CancellationToken cancellationToken);
    public Task<IEnumerable<MemberDto>> GetFollowMembersAsync(string? loggedInUserEmail, string predicate, CancellationToken cancellationToken);
}
