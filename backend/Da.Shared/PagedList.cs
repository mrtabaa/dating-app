namespace Da.Shared;

public class PagedList<T> : List<T>
{
    // set props values
    public PagedList(IEnumerable<T> items, int itemsCount, int pageNumber, int pageSize)
    {
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(itemsCount / (double)pageSize);
        PageSize = pageSize;
        TotalItemsCount = itemsCount;
        AddRange(items);
    }

    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalItemsCount { get; set; }
}