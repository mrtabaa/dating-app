namespace api.Settings;

public class IJwtSettings
{
    private string Issuer { get; init; } = string.Empty;
    private string Audience { get; init; } = string.Empty;
    private string Key { get; init; } = string.Empty;
}