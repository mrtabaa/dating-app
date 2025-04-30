using System.Runtime.InteropServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace da.Infrastructure.Mongo.Models;

public record Follow(
    string? Schema,
    [Optional][property: BsonId, BsonRepresentation(BsonType.ObjectId)] ObjectId Id,
    ObjectId FollowerId, // loggedInUser who follows others
    ObjectId FollowedMemberId // the user who's followed by loggedInUser
);
