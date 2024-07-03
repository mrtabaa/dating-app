namespace api.Interfaces;

public interface IMessageRepository
{
    public Task<MessageStatus> CreateAsync(ObjectId userId, MessageInDto messageInDto, CancellationToken cancellationToken);
}
