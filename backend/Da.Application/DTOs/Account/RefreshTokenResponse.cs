namespace Da.Application.DTOs.Account;

public record RefreshTokenResponse(
    string TokenValueRaw,
    string JtiValue,
    DateTimeOffset ExpiresAt
);