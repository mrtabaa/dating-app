namespace api.Interfaces;

public interface IFollowRepository
{
    public Task<FollowStatus> AddFollowAsync(string userIdHashed, string targetMemberUserName, CancellationToken cancellationToken);
    public Task<FollowStatus> RemoveFollowAsync(string userIdHashed, string followedMemberUserName, CancellationToken cancellationToken);
    public Task<PagedList<AppUser>?> GetFollowMembersAsync(string userIdHashed, FollowParams followParams, CancellationToken cancellationToken);
    public Task<bool> CheckIsFollowing(ObjectId userId, AppUser appUser, CancellationToken cancellationToken);
}
