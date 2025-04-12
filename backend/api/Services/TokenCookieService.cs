using Microsoft.AspNetCore.DataProtection;

namespace api.Services;

public class TokenCookieService(IDataProtectionProvider provider) : ITokenCookieService
{
    private readonly IDataProtector _protector = provider.CreateProtector("RefreshTokenCookieProtector-v1");

    public string EncryptRefreshTokenResponse(RefreshTokenResponse response)
    {
        string json = JsonSerializer.Serialize(response);
        return _protector.Protect(json);
    }

    public RefreshTokenRequest DecryptRefreshTokenRequest(string protectedCookieValue)
    {
        string json = _protector.Unprotect(protectedCookieValue);
        return JsonSerializer.Deserialize<RefreshTokenRequest>(json)!;
    }
}