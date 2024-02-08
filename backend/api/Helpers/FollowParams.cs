namespace api.Helpers;

public class FollowParams : PaginationParams
{
    public ObjectId? LoggedInUserId { get; set; }
    public string Predicate { get; set; } = "followings";
}
