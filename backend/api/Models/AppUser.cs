namespace api.Models;

public record AppUser(
    string? Schema,
    ObjectId? Id,
    string Email,
    byte[]? PasswordHash,
    byte[]? PasswordSalt,
    DateOnly DateOfBirth,
    string KnownAs,
    DateTime Created,
    DateTime LastActive,
    string Gender,
    string? Introduction,
    string? LookingFor,
    string? Interests,
    string City,
    string Country,
    List<Photo> Photos, // TODO limit to 100 photos
    int Liked_byCount
// int FriendsCount // TODO pre-calc number of friends
);
