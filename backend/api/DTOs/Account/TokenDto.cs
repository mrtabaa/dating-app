namespace api.DTOs.Account;

public record TokenDto(
    string AccessToken,
    RefreshTokenDto RefreshTokenDto
);

public record RefreshTokenDto(
    string RefreshToken,
    DateTime ExpiresAt
);