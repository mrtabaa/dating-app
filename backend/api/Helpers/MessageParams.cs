namespace api.Helpers;

public class MessageParams : PaginationParams
{
    public MessagePredicate Predicate { get; init; } = MessagePredicate.Inbox;
    public string TargetUserName { get; init; } = string.Empty;
}
