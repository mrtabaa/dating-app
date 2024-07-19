namespace api.DTOs;

public record CreatedMessageDto(
    string TempId, // For optimistic approach in client
    string Id, // Use to update/delete message
    string Content,
    DateTime SentOn,
    DateTime? ReadOn
);
