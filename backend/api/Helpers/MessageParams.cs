namespace api.Helpers;

public class MessageParams : PaginationParams
{
    public MessagePredicate Predicate { get; set; } = MessagePredicate.Inbox;
}
