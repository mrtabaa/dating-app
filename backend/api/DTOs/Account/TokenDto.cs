namespace api.DTOs.Account;

public record TokenDto(
    string AccessToken,
    string RefreshToken
);