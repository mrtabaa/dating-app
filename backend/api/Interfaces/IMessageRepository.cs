namespace api.Interfaces;

public interface IMessageRepository
{
    public Task<MessageDto?> CreateAsync(ObjectId userId, MessageInDto messageInDto, CancellationToken cancellationToken);
    public Task<PagedList<Message>> GetAsync(ObjectId userId, MessageParams messageParams, CancellationToken cancellationToken);
    public Task<PagedList<Message>?> GetThreadAsync(ObjectId userId, MessageParams messageParams, CancellationToken cancellationToken);
    public Task<DateTime?> UpdateReadOnAsync(ObjectId userId, ObjectId targetUserId, CancellationToken cancellationToken);
}