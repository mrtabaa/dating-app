namespace da.Application.DTOs.Parameters;

public class MessageParams : PaginationParams
{
    public MessagePredicate Predicate { get; set; } = MessagePredicate.Inbox;
    public string TargetUserName { get; set; } = string.Empty;
}