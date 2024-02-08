namespace api.Interfaces;

public interface IFollowRepository
{
    public Task<FolowStatus> AddFollowAsync(string? loggedInUserEmail, string targetMemberEmail, CancellationToken cancellationToken);
    public Task<PagedList<AppUser>> GetFollowMembersAsync(string loggedInUserEmail, FollowParams followParams, CancellationToken cancellationToken);
}
