namespace api.SignalR.Interfaces;

public interface IMessageService
{
    public Task<bool> AddGroupNameAsync(ObjectId userId, string groupName, CancellationToken cancellationToken);
    public Task<HashSet<string>?> GetGroupNamesAsync(ObjectId userId, CancellationToken cancellationToken);
    public Task<bool> RemoveGroupNameAsync(ObjectId userId, string groupName, CancellationToken cancellationToken);    
}
