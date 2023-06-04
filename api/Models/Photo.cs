namespace api.Models;

public record Photo(
    string? Schema,
    string Url,
    bool IsMain
);
