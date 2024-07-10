namespace api.DTOs;

public record MessageDto(
    string Id, // Use to update/delete message
    string? UserOrTargetUserName,
    string? UserOrTargetKnownAs,
    string? UserOrTargetProfilePhoto,
    string Content,
    DateTime SentOn,
    DateTime? ReadOn
);
