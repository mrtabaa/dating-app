namespace api.DTOs;

public class MessageDto
{
    required public string Id { get; init; }
    required public string? UserOrTargetUserName { get; init; }
    required public string? UserOrTargetKnownAs { get; init; }
    required public string? UserOrTargetProfilePhoto { get; init; }
    required public string Content { get; init; }
    required public DateTime SentOn { get; init; }
    required public DateTime? ReadOn { get; set; }
}
