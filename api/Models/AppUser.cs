namespace api.Models;

public record AppUser(
    int Schema,
    [property: BsonId, BsonRepresentation(BsonType.ObjectId)] string? Id,
    string Name,
    string Email,
    byte[] PasswordSalt,
    byte[] PasswordHash
);
