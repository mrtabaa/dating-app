namespace api.Interfaces;

public interface ILikesRepository
{
    public Task<bool> AddLikeAsync(string? loggedInUserId, string targetMemberId, CancellationToken cancellationToken);
    // public Task<IEnumerable<Like>> GetLikedMembersAsync(string? loggedInUserId, string targetMemberId, CancellationToken cancellationToken);
}
