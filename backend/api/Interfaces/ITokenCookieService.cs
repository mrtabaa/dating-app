namespace api.Interfaces;

public interface ITokenCookieService
{
    public string EncryptRefreshTokenResponse(RefreshTokenResponse refreshTokenResponse);
    public RefreshTokenRequest DecryptRefreshTokenRequest(string protectedRefreshToken);
}