namespace Da.Application.DTOs;

public class MessageDto
{
    public required string Id { get; init; }
    public required string? UserOrTargetUserName { get; init; }
    public required string? UserOrTargetKnownAs { get; init; }
    public required string? UserOrTargetProfilePhoto { get; init; }
    public required string Content { get; init; }
    public required DateTime SentOn { get; init; }
    public required DateTime? ReadOn { get; set; }
}