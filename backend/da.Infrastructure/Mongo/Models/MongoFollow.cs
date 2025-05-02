namespace da.Infrastructure.Mongo.Models;

public record MongoFollow(
    string? Schema,
    [Optional]
    [property: BsonId]
    [property: BsonRepresentation(BsonType.ObjectId)]
    ObjectId Id,
    ObjectId FollowerId, // loggedInUser who follows others
    ObjectId FollowedMemberId // the user who's followed by loggedInUser
);