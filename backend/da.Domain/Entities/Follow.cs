namespace da.Domain.Entities;

public record Follow(
    string? Schema,
    string FollowerId, // loggedInUser who follows others
    string FollowedMemberId // the user who's followed by loggedInUser
);