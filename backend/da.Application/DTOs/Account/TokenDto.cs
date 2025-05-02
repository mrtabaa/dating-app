namespace da.Application.DTOs.Account;

public record TokenDto(
    string AccessToken, // JWT
    RefreshTokenResponse RefreshTokenResponse
);