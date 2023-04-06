namespace api.DTOs;

public record PhotoDto(
    string Schema,
    string Id,
    string Url,
    bool IsMain
);
