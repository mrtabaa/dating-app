namespace api.Interfaces;

public interface ILikeRepository
{
    public Task<LikeStatus> AddLikeAsync(string? loggedInUserEmail, string targetMemberEmail, CancellationToken cancellationToken);
    public Task<IEnumerable<MemberDto>> GetLikeMembersAsync(string? loggedInUserEmail, string predicate, CancellationToken cancellationToken);
}
