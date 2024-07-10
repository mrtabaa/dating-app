namespace api.Helpers;

public class MessageParams : PaginationParams
{
    public MessagePredicate Predicate { get; init; } = MessagePredicate.Inbox;
    public string targetUserName { get; init; } = string.Empty;
}
