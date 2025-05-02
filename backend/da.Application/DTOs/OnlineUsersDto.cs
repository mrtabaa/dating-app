namespace da.Application.DTOs;

public record OnlineUsersDto(
    string UserName,
    DateTimeOffset LastActive
);