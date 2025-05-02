using System.Runtime.InteropServices;

namespace Da.Domain.Entities;

public record Message(
    string? Schema,
    [Optional] string Id,
    string SenderId,
    string ReceiverId,
    string Content,
    DateTime SentOn,
    DateTime? ReadOn,
    bool SenderDeleted,
    bool ReceiverDeleted
);