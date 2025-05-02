namespace da.Infrastructure.Mongo.Models;

[CollectionName("roles")]
public class MongoAppRole : MongoIdentityRole<ObjectId>
{
}