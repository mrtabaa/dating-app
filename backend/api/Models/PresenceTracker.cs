namespace api.Models;

public class PresenceTracker
{
    public string? Schema { get; set; }
    [property: BsonId, BsonRepresentation(BsonType.ObjectId)] public ObjectId Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<string> ConnectionIds { get; set; } = [];
}
