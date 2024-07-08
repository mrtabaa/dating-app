namespace api.Interfaces;

public interface IMessageRepository
{
    public Task<MessageStatus> CreateAsync(ObjectId userId, MessageInDto messageInDto, CancellationToken cancellationToken);
    public Task<PagedList<Message>> GetInboxMessagesAsync(ObjectId userId, PaginationParams paginationParams, CancellationToken cancellationToken);
}
