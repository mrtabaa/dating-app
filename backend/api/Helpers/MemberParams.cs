namespace api.Helpers;

public class MemberParams
{
    private const int MaxPageSize = 25;

    public int PageNumber { get; init; } = 1;

    private int _pageSize = 5;
    public int PageSize
    {
        get { return _pageSize; }
        set { _pageSize = value > MaxPageSize ? MaxPageSize : value; }
    }

    public string? LoggedInUserId { get; set; }
    public string? Gender { get; set; }
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 100;
    public string OrderBy { get; set; } = "lastActive";
}
