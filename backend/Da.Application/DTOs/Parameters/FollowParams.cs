using Da.Application.Enums;

namespace Da.Application.DTOs.Parameters;

public class FollowParams : PaginationParams
{
    public string? UserId { get; set; }
    public FollowPredicate Predicate { get; set; } = FollowPredicate.Followings;
}