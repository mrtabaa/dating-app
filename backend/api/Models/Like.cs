namespace api.Models;

public record Like(
    string? Schema,
    ObjectId? Id,
    ObjectId? LoggedInUserId, // loggedInUser who likes others
    ObjectId? TargetMemberId // the user who's liked by loggedInUser
);
