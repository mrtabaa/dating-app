namespace api.Models.Helpers;

public record MessageStatus(
    [Optional] bool IsReceiverNotFound,
    [Optional] MessageDto MessageDto
);
