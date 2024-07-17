namespace api.DTOs;

public record CreatedMessageDto(
    string Id, // Use to update/delete message
    string Content,
    DateTime SentOn,
    DateTime? ReadOn
);
