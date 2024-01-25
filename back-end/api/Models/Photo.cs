namespace api.Models;

public record Photo(
    string? Schema,
    string Url_165, // navbar & thumbnail
    string Url_256, // card
    string Url_enlarged, // enlarged photo up to ~300kb
    bool IsMain
);
