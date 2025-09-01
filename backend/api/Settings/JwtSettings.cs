namespace api.Settings;

public class JwtSettings : IJwtSettings
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string TokenKey { get; init; } = string.Empty;
}