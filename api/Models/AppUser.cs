namespace api.Models;

public record AppUser(
    string? Schema,
    [property: BsonId, BsonRepresentation(BsonType.ObjectId)] string? Id,
    string Name,
    string Email,
    string Password,
    byte[]? PasswordHash,
    byte[]? PasswordSalt,
    DateOnly DateOfBirth,
    string ConfirmPassword,
    string KnownAs,
    DateTime Created,
    DateTime LastActive,
    string Gender,
    string Introduction,
    string LookingFor,
    string Interests,
    string City,
    string Country,
    List<Photo> Photos
);
