namespace da.Application.DTOs;

public class MessageInDto
{
    public required string TempId { get; init; }
    public required string Content { get; set; }
    public required string ReceiverUserName { get; init; }
}