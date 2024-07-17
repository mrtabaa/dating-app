namespace api.DTOs;

public record MessageInDto(
    [Length(1, 500)] string Content,
    [Length(1, 50)] string ReceiverUserName
);
