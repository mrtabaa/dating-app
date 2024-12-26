namespace api.SignalR.Interfaces;

public interface IMessageService
{
    public Task<bool> AddMessageGroupAsync(ObjectId userId, MessageGroup messageGroup, CancellationToken cancellationToken);
    public Task<MessageGroup?> GetMessageGroupAsync(ObjectId userId, string connectionId, CancellationToken cancellationToken);
    public Task<bool> RemoveMessageGroupAsync(ObjectId userId, MessageGroup messageGroup);
    public Task<bool> CheckIsMemberIsInGroupAsync(ObjectId userId, string groupNameIn, CancellationToken cancellationToken);
}