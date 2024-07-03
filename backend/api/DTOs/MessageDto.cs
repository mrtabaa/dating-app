namespace api.DTOs;

public record MessageDto(
    string Id,
    string SenderUserName,
    string ReceiverUserName,
    string Content,
    DateTime? ReadOn,
    DateTime SentOn
);
