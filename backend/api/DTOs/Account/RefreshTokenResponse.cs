namespace api.DTOs.Account;

public record RefreshTokenResponse(
    string TokenValueRaw,
    string JtiValue,
    DateTimeOffset ExpiresAt
);