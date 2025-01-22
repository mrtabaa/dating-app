namespace api.DTOs.Helpers;

public record OperationResult<T>(
    bool IsSuccess,
    [Optional] T Result,
    [Optional] CustomError Error
);

public record OperationResult(
    bool IsSuccess,
    [Optional] CustomError Error
);

public record CustomError(
    Enum Code,
    string? Message
);