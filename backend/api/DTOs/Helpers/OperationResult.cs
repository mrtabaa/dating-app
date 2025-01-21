namespace api.DTOs.Helpers;

public record OperationResult<T>(
    [Optional] bool IsSuccess,
    [Optional] T Result,
    [Optional] CustomError Error
);

public record CustomError(
    Enum Code,
    string? Message
);