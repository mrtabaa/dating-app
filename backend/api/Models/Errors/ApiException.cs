namespace api.Models.Errors;

public class ApiException
{
    [property: BsonId, BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }
    
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public DateTime Time { get; set; }
};
