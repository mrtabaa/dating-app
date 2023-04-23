namespace api.DTOs;

public record PhotoDto(
    string Schema,
    string Url,
    bool IsMain
);
