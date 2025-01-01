namespace api.Controllers.Helpers;

[Produces("application/json")]
public class BuggyController : BaseApiController
{
    private const string CollectionName = "Users";
    private readonly IMongoCollection<AppUser>? _collection;

    public BuggyController(IMongoClient client, IMyMongoDbSettings dbSettings)
    {
        IMongoDatabase? database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AppUser>(CollectionName);
    }

    [HttpGet("azure")]
    public ActionResult AzureAccessTest() => Ok("Connected to Azure");

    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> GetSecret() => "Secret Text"; // Doesn't return the value due to auth

    [HttpGet("not-found")]
    public ActionResult<AppUser> GetNotFound()
    {
        AppUser thing = _collection.Find(user => user.Email == "no email").FirstOrDefault();
        if (thing is null) return NotFound(); // return 404
        return thing;
    }

    [HttpGet("server-error")]
    public ActionResult<string>? GetServerError()
    {
        AppUser thing = _collection.Find(user => user.Email == "no email").FirstOrDefault();

        if (string.IsNullOrEmpty(thing.Email)) return null;

        return thing.Email.ToUpper(); //return 500
    }

    [HttpGet("bad-request")]
    public ActionResult<string> GetBadRequest() => BadRequest("This was a bad request."); // return 400
}