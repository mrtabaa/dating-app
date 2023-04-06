namespace api.Models
{
    public record Photo
    (
        int Schema,
        [property: BsonId, BsonRepresentation(BsonType.ObjectId)] string? Id,
        string Url,
        bool IsMain,
        string PublicId
    );
}