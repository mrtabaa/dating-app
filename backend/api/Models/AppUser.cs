using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace api.Models;

[CollectionName("users")]
public class AppUser : MongoIdentityUser<ObjectId>
{
    public string? IdentifierHash { get; init; }
    public string? JtiValue { get; set; }
    public string? Schema { get; init; }
    public DateOnly DateOfBirth { get; init; }
    public string KnownAs { get; init; } = string.Empty;
    public DateTime LastActive { get; init; }
    public string Gender { get; init; } = string.Empty;
    public string? Introduction { get; init; }
    public string? LookingFor { get; init; }
    public string? Interests { get; init; }
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public List<Photo> Photos { get; init; } = []; // TODO limit to 100 photos
    public int FollowersCount { get; init; }
    public int FriendsCount { get; init; } // TODO pre-calc number of friends
}