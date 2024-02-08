namespace api.Helpers;

public class PaginationParams
{
    private const int MaxPageSize = 25;

    public int PageNumber { get; init; } = 1;

    private int _pageSize = 5;
    public int PageSize
    {
        get { return _pageSize; }
        set { _pageSize = value > MaxPageSize ? MaxPageSize : value; }
    }
}
