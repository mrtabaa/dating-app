namespace api.Helpers;

public class PagedList<T> : List<T>
{
    public PagedList() // allows creating an empty object without constructor's arguments
    {
    }

    // set props values
    public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
    {
        CurrentPage = pageNumber;
        TotalPages = (int) Math.Ceiling(count / (double) pageSize);
        PageSize = pageSize;
        TotalCount = count;
        AddRange(items);
    } 

    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    /// <summary>
    /// call MongoDB collection and get a limited number of items based on the pageSize and pageNumber.
    /// </summary>
    /// <param name="source"></param>: getting a query to use agains MongoDB _collection
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>PageList<T> object with its prop values</returns>
    public static async Task<PagedList<T>> CreateAsync(IMongoQueryable<T>? source, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
