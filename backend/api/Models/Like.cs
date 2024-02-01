namespace api.Models;

public record Like(
    string? Schema,
    [property: BsonId, BsonRepresentation(BsonType.ObjectId)] string? Id,
    LoggedInUser LoggedInUser,
    TargetMember TargetMember
);

public record LoggedInUser(
    string LoggedInUserId, // loggedInUser who likes others
    string Email,
    int Age,
    string KnownAs,
    string Gender,
    string City,
    string? PhotoUrl
);

public record TargetMember(
    string TargetMemberId, // the user who's liked by loggedInUser
    string Email,
    int Age,
    string KnownAs,
    string Gender,
    string City,
    string? PhotoUrl
);

