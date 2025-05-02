namespace Da.Infrastructure.Mongo.Models;

public record MongoMessage(
    string? Schema,
    [Optional]
    [property: BsonId]
    [property: BsonRepresentation(BsonType.ObjectId)]
    ObjectId Id,
    ObjectId SenderId,
    ObjectId ReceiverId,
    string Content,
    DateTime SentOn,
    DateTime? ReadOn,
    bool SenderDeleted,
    bool ReceiverDeleted
);