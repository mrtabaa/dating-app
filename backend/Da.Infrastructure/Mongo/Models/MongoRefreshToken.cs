namespace Da.Infrastructure.Mongo.Models;

public class MongoRefreshToken
{
    [property: BsonId]
    [property: BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    public ObjectId UserId { get; init; }
    public string JtiValue { get; init; } = string.Empty;
    public string TokenValueHashed { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }

    public DateTimeOffset? UsedAt { get; set; } // Token is used before and not allowed again
    public bool IsRevoked { get; set; } // Admin revoked session manually OR user logout

    [Required] public SessionMetadata? SessionMetadata { get; init; }
}