namespace api.Models;

public record Like(
    string? Schema,
    [property: BsonId, BsonRepresentation(BsonType.ObjectId)] ObjectId Id,
    ObjectId? LikerId, // loggedInUser who likes others
    ObjectId? LikedId // the user who's liked by loggedInUser
);
