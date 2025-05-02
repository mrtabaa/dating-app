namespace da.Application.DTOs;

public record OperationResult<T>(
    bool IsSuccess,
    [Optional] T Result,
    CustomError? Error
);

public record OperationResult(
    bool IsSuccess,
    CustomError? Error
);

public record CustomError(
    Enum Code,
    [Optional] string? Message
);