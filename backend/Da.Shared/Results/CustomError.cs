namespace Da.Shared.Results;

public record CustomError(
    Enum Code,
    [Optional] string? Message
);