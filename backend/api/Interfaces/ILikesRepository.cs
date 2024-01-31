namespace api.Interfaces;

public interface ILikesRepository
{
    public Task<LikeStatus> AddLikeAsync(string? loggedInUserId, string targetMemberId, CancellationToken cancellationToken);
    public Task<List<Like>?> GetLikedMembersAsync(string? loggedInUserId, string predicate, CancellationToken cancellationToken);
}
