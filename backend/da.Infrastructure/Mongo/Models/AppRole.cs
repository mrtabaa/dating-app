using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;
using MongoDbGenericRepository.Attributes;

namespace da.Infrastructure.Mongo.Models;

[CollectionName("roles")]
public class AppRole : MongoIdentityRole<ObjectId>
{
}
