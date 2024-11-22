namespace api.SignalR.Interfaces;

public interface IMessageService
{
    public Task<bool> AddGroupNameAsync(ObjectId userId, string groupName, CancellationToken cancellationToken);
    public Task<bool> CheckIsMemberInGroupAsync(ObjectId userId, string groupNameIn, CancellationToken cancellationToken);
    public Task<bool> RemoveGroupNameFromDbAsync(ObjectId userId, string groupName, CancellationToken cancellationToken);
}