namespace Da.Shared.Results;

public record OperationResult<T>(
    bool IsSuccess,
    [Optional] T Result,
    CustomError? Error
);

public record OperationResult(
    bool IsSuccess,
    CustomError? Error
);
