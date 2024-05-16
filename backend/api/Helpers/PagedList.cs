namespace api.Helpers;

public class PagedList<T> : List<T>
{
    // set props values
    private PagedList(IEnumerable<T> items, int itemsCount, int pageNumber, int pageSize)
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

    /// <summary>
    /// call MongoDB collection and get a limited number of items based on the pageSize and pageNumber.
    /// </summary>
    /// <param name="query"></param>: getting a query to use agains MongoDB _collection
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>PageList<T> object with its prop values</returns>
    public static async Task<PagedList<T>> CreatePagedListAsync(IMongoQueryable<T> query, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        int count = await query.CountAsync(cancellationToken);
        IEnumerable<T> items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
