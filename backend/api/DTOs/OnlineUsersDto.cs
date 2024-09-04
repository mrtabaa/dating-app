namespace api.DTOs;

public record OnlineUsersDto(
    string UserName,
    DateTime LastActive
);