namespace api.Interfaces;

public interface IFollowRepository
{
    public Task<FolowStatus> AddFollowAsync(string? userIdHashed, string targetMemberEmail, CancellationToken cancellationToken);
    public Task<PagedList<AppUser>?> GetFollowMembersAsync(string userIdHashed, FollowParams followParams, CancellationToken cancellationToken);
    public Task<bool> CheckIsFollowing(ObjectId userId, AppUser appUser, CancellationToken cancellationToken);
}
