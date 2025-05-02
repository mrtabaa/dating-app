namespace da.Infrastructure.Mongo.Models;

public class MongoApiException
{
    [property: BsonId]
    [property: BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public DateTime Time { get; set; }
}