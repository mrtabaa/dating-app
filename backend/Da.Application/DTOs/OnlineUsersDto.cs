namespace Da.Application.DTOs;

public record OnlineUsersDto(
    string UserName,
    DateTimeOffset LastActive
);