namespace api.Extensions;

public static class HttpExtensions
{
    public static void AddPaginationHeader(this HttpResponse response, PaginationHeader header)
    {
        var jsonOptions = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase}; // format to json format
        response.Headers.Append("Pagination", JsonSerializer.Serialize(header, jsonOptions)); // adding header to response
        response.Headers.Append("Access-Control-Expose-Headers", "Pagination"); // allow CORS policy
    }
}
