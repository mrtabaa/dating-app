namespace api.DTOs;

public record MessageInDto
{
    public required string TempId { get; init; }
    public required string Content { get; set; }
    public required string ReceiverUserName { get; init; }
}