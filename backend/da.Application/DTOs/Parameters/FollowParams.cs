namespace da.Application.DTOs.Parameters;

public class FollowParams : PaginationParams
{
    public string? UserId { get; set; }
    public FollowPredicate Predicate { get; set; } = FollowPredicate.Followings;
}