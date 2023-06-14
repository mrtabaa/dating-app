namespace api.Models;

public record Photo(
    string? Schema,
    string Url_128,
    string Url_512,
    string Url_1024,
    bool IsMain
);
