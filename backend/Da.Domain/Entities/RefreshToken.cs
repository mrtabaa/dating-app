namespace api.Models.Helpers;

public class RefreshToken
{
    public string Id { get; set; } = string.Empty;

    public string UserId { get; init; } = string.Empty;
    public string JtiValue { get; init; } = string.Empty;
    public string TokenValueHashed { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }

    public DateTimeOffset? UsedAt { get; set; } // Token is used before and not allowed again
    public bool IsRevoked { get; set; } // Admin revoked session manually OR user logout

    [Required] public SessionMetadata? SessionMetadata { get; init; }
}