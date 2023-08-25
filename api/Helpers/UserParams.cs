namespace api.Helpers;

public class UserParams
{
    private const int MaxPageSize = 50;

    public int PageNumber { get; init; } = 1;

    private int _pageSize = 10;
    public int PageSize
    {
        get { return _pageSize; }
        set { _pageSize = (value > MaxPageSize) ? MaxPageSize : value; }
    }
}
