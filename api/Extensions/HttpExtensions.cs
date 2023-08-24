using System.Text.Json;

namespace api.Extensions;

public static class HttpExtensions
{
    public static void AddPaginationHeader(this HttpResponse response, PaginationHeader header)
    {
        var jsonOptions = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase}; // format to json format
        response.Headers.Add("Pagination", JsonSerializer.Serialize(header, jsonOptions)); // adding header to response
        response.Headers.Add("Access-Control-Expose-Headers", "Pagination"); // allow CORS policy
    }
}
