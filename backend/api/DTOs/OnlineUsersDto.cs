namespace api.DTOs;

public record OnlineUsersDto(
    string UserName,
    DateTimeOffset LastActive
);