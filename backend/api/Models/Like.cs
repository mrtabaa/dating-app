namespace api.Models;

public record Like(
    string? Schema,
    [property: BsonId, BsonRepresentation(BsonType.ObjectId)] string? Id,
    string LoggedInUserId, // loggedInUser who likes others
    string TargetMemberId // the user who's liked by loggedInUser
);
