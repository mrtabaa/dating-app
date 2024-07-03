namespace api.Models.Helpers;

public record MessageStatus(
    [Optional] bool IsSuccess,
    [Optional] bool IsUnauthorized,
    [Optional] bool IsTargetMemberNotFound
);
