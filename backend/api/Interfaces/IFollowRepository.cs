namespace api.Interfaces;

public interface IFollowRepository
{
    public Task<FollowStatus> AddFollowAsync(ObjectId userId, string targetMemberUserName, CancellationToken cancellationToken);
    public Task<FollowStatus> RemoveFollowAsync(ObjectId userId, string followedMemberUserName, CancellationToken cancellationToken);
    public Task<PagedList<AppUser>?> GetFollowMembersAsync(ObjectId userId, FollowParams followParams, CancellationToken cancellationToken);
    public Task<bool> CheckIsFollowing(ObjectId userId, AppUser appUser, CancellationToken cancellationToken);
}
