namespace api.Interfaces;

public interface IFollowRepository
{
    public Task<OperationResult<PagedList<AppUser>>> GetFollowMembersAsync(
        ObjectId userId, FollowParams followParams, CancellationToken cancellationToken
    );

    public Task<OperationResult> AddFollowAsync(
        ObjectId userId, string targetMemberUserName, CancellationToken cancellationToken
    );

    public Task<OperationResult> RemoveFollowAsync(
        ObjectId userId, string followedMemberUserName, CancellationToken cancellationToken
    );

    public Task<bool> CheckIsFollowing(ObjectId userId, AppUser appUser, CancellationToken cancellationToken);
}