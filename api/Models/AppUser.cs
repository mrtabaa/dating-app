using MongoDB.Bson.Serialization.Attributes;

namespace api.Models;

public record AppUser(
    int Schema,
    [property: BsonId, BsonRepresentation(BsonType.ObjectId)] string? Id,
    string Email,
    byte[] PasswordSalt,
    byte[] PasswordHash
);
