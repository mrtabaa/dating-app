namespace Da.Infrastructure.Mongo.Models;

[CollectionName("users")]
public class MongoAppUser : MongoIdentityUser<ObjectId>
{
    public string Schema { get; init; } = string.Empty;
    public string? IdentifierHash { get; init; }
    public DateOnly DateOfBirth { get; set; }
    public string KnownAs { get; init; } = string.Empty;
    public DateTimeOffset LastActive { get; init; }
    public string Gender { get; set; } = string.Empty;
    public string? Introduction { get; init; }
    public string? LookingFor { get; init; }
    public string? Interests { get; init; }
    public string CountryAcr { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public List<Photo> Photos { get; set; } = [];
    public int FollowingsCount { get; init; }
    public int FollowersCount { get; init; }
    public bool IsProfileCompleted { get; init; }
    public List<string> ConnectionsPresence { get; set; } = [];
    public List<MessageGroup> MessageGroups { get; set; } = [];
}