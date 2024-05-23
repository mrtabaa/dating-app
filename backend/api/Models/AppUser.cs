using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace api.Models;

[CollectionName("users")]
public class AppUser : MongoIdentityUser<ObjectId>
{
    public string? IdentifierHash { get; init; }
    public string? JtiValue { get; init; }
    public string Schema { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
    public string KnownAs { get; init; } = string.Empty;
    public DateTime LastActive { get; init; }
    public string Gender { get; init; } = string.Empty;
    public string? Introduction { get; init; }
    public string? LookingFor { get; init; }
    public string? Interests { get; init; }
    public string Country { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public List<Photo> Photos { get; set; } = [];
    public int FollowingsCount { get; init; }
    public int FollowersCount { get; init; }
    public bool IsProfileCompleted { get; init; }
}