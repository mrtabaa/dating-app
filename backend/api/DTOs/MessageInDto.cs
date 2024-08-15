namespace api.DTOs;

public record MessageInDto(
    string TempId,
    [Length(1, 500)] string Content,
    string ReceiverUserName
);
