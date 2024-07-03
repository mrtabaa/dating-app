namespace api.Models;

public record Message(
    string? Schema,
    [property: BsonId, BsonRepresentation(BsonType.ObjectId)] ObjectId Id,
    ObjectId SenderId,
    string SenderUserName,
    ObjectId RecieverId,
    string ReceiverUserName,
    string Content,
    DateTime SentOn,
    DateTime? ReadOn,
    bool SenderDeleted,
    bool ReceiverDeleted
);
