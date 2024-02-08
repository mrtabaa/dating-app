namespace api.Helpers;

public class MemberParams : PaginationParams
{
    public ObjectId? LoggedInUserId { get; set; }
    public string? Gender { get; set; }
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 100;
    public string OrderBy { get; set; } = "lastActive";
}
