namespace api.Interfaces;

public interface ILikesRepository
{
    public Task<LikeStatus> AddLikeAsync(string? loggedInUserEmail, string targetMemberEmail, CancellationToken cancellationToken);
    public Task<List<LikeDto>> GetLikedMembersAsync(string? loggedInUserEmail, string predicate, CancellationToken cancellationToken);
}
