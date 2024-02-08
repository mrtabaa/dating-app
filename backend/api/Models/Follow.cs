namespace api.Models;

public record Follow(
    string? Schema,
    [property: BsonId, BsonRepresentation(BsonType.ObjectId)] ObjectId Id,
    ObjectId? FollowerId, // loggedInUser who follows others
    ObjectId? FollowedId // the user who's followed by loggedInUser
);
