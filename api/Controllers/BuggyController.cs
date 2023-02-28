namespace api.Controllers;

public class BuggyController : BaseApiController {

    const string _collectionName = "Users";
    private readonly IMongoCollection<AppUser>? _collection;

    public BuggyController(IMongoClient client, IMongoDbSettings dbSettings) {
        var database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AppUser>(_collectionName);
    }

    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> GetSecret() {
        return "Secret Text";
    }

    [HttpGet("not-found")]
    public ActionResult<string> GetNotFound() {
        AppUser thing = _collection.Find<AppUser>(user => user.Email == "no email").FirstOrDefault();
        return thing == null ? "User not found" : "Found user, no error.";
    }

    [HttpGet("server-error")]
    public ActionResult<string> GetServerError() {
        AppUser thing = _collection.Find<AppUser>(user => user.Email == "no email").FirstOrDefault();

        return thing.Email.ToUpper();
    }

    [HttpGet("bad-request")]
    public ActionResult<string> GetBadRequest() {
        return BadRequest("This was a bad request.");
    }
}
