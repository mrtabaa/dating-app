namespace api.Interfaces;

public interface ILikeRepository
{
    public Task<LikeStatus> AddLikeAsync(string? loggedInUserEmail, string targetMemberEmail, CancellationToken cancellationToken);
    public Task<List<MemberDto>> GetLikedMembersAsync(string? loggedInUserEmail, string predicate, CancellationToken cancellationToken);
}
