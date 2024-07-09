namespace api.DTOs;

public record MessageDto(
    string Id, // Use to update/delete message
    string? SenderUserName,
    string? ReceiverUserName,
    string? TargetUserProfilePhoto,
    string Content,
    DateTime SentOn,
    DateTime? ReadOn
);
