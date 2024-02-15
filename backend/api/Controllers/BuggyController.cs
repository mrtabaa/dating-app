namespace api.Controllers;

[Produces("application/json")]
public class BuggyController : BaseApiController
{

    const string _collectionName = "Users";
    private readonly IMongoCollection<AppUser>? _collection;

    public BuggyController(IMongoClient client, IMyMongoDbSettings dbSettings)
    {
        var database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AppUser>(_collectionName);
    }

    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> GetSecret()
    {
        return "Secret Text"; // doen't return the value due to auth
    }

    [HttpGet("not-found")]
    public ActionResult<AppUser> GetNotFound()
    {
        AppUser thing = _collection.Find<AppUser>(user => user.Email == "no email").FirstOrDefault();
        if (thing is null)
        {
            return NotFound(); // return 404
        }
        return thing;
    }

    [HttpGet("server-error")]
    public ActionResult<string> GetServerError()
    {
        AppUser thing = _collection.Find<AppUser>(user => user.Email == "no email").FirstOrDefault();

        return thing.Email.ToUpper(); //return 500
    }

    [HttpGet("bad-request")]
    public ActionResult<string> GetBadRequest()
    {
        return BadRequest("This was a bad request."); // return 400
    }
}
