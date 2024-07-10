namespace api.Models.Helpers;

public record MessageStatus(
    [Optional] bool IsReceiverNotFound,
    [Optional] bool IsLoggedInUserNotFound,
    [Optional] MessageDto MessageDto
);
