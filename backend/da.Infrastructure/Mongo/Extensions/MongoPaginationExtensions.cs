namespace da.Infrastructure.Mongo.Extensions;

public static class MongoPaginationExtensions
{
    /// <summary>
    ///     call MongoDB collection and get a limited number of items based on the pageSize and pageNumber.
    /// </summary>
    /// <param name="query"></param>
    /// getting a query to use against MongoDB _collection
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns T="object with its prop values">PageList</returns>
    public static async Task<PagedList<T>> CreatePagedListAsync<T>(
        this IMongoQueryable<T> query, int pageNumber, int pageSize, CancellationToken cancellationToken
    )
    {
        int count = await query.CountAsync(cancellationToken);
        IEnumerable<T> items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).
            ToListAsync(cancellationToken);

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}