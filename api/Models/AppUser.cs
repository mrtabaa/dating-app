namespace api.Models;
public class AppUser {
    public int Schema { get; init; }
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; init; }
    public string? Email { get; set; }
    public int Power { get; set; }
    public string? Password { get; set; }
    public byte[]? PasswordHash { get; set; }
    public byte[]? PasswordSalt { get; set; }
    public bool Verified { get; set; }
    public string? Name { get; set; }
    public string[]? PhotoUrls { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? Token { get; set; }

}
