namespace Da.Application.DTOs.Account;

public class RefreshTokenRequest
{
    [Length(10, 256)] public string TokenValueRaw { get; init; } = string.Empty;

    [Length(10, 128)] public string JtiValue { get; init; } = string.Empty;

    public SessionMetadata? SessionMetadata { get; set; }
}