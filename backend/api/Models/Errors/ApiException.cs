namespace api.Models.Errors;

public record ApiException(
    [property: BsonId, BsonRepresentation(BsonType.ObjectId)] ObjectId Id,
    int StatusCode,
    string Message,
    string? Details,
    DateTime Time
);
