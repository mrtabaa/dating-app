using Microsoft.AspNetCore.Identity;

namespace api.Services;
public class TokenService : ITokenService
{
    private readonly IMongoCollection<AppUser> _collection;
    private readonly SymmetricSecurityKey? _key; // set it as nullable by ? mark
    private readonly UserManager<AppUser> _userManager;

    public TokenService(IConfiguration config, IMongoClient client, IMyMongoDbSettings dbSettings, UserManager<AppUser> userManager)
    {
        var database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AppUser>(AppVariablesExtensions.collectionUsers);

        string? tokenValue = config[AppVariablesExtensions.TokenKey];

        // throw exception if tokenValue is null
        _ = tokenValue ?? throw new ArgumentNullException("tokenValue cannot be null", nameof(tokenValue));

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenValue!));

        _userManager = userManager;
    }

    public async Task<string?> CreateToken(AppUser user, CancellationToken cancellationToken)
    {
        string jtiValue = Guid.NewGuid().ToString();

        string? identifierHash = await InsertEncryptedUserId(user.Id, jtiValue, cancellationToken); // this securedId is stored in users collection to associate with the AppUser.

        if (!string.IsNullOrEmpty(identifierHash))
        {
            var claims = new List<Claim> {
            new(JwtRegisteredClaimNames.NameId, identifierHash), // unique user Id for identification.
            new(JwtRegisteredClaimNames.Jti, jtiValue), // TODO store in db/cache to prevent multiple login sessions with one token. If already exists, reject new login.
            };

            // Get user's roles and add them all into claims
            IList<string>? roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7), // Set expiration days
                SigningCredentials = creds,
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        return null;
    }

    private async Task<string?> InsertEncryptedUserId(ObjectId userId, string jtiValue, CancellationToken cancellationToken)
    {
        string newObjectId = ObjectId.GenerateNewId().ToString();

        string identifierHash = BCrypt.Net.BCrypt.HashPassword(newObjectId);

        UpdateDefinition<AppUser> updatedSecuredToken = Builders<AppUser>.Update
            .Set(appUser => appUser.IdentifierHash, identifierHash)
            .Set(appUser => appUser.JtiValue, jtiValue);

        UpdateResult updateResult = await _collection.UpdateOneAsync<AppUser>(appUser => appUser.Id == userId, updatedSecuredToken, null, cancellationToken);

        if (updateResult.ModifiedCount == 1)
            return identifierHash;

        return null;
    }

    /// <summary>
    /// Get a hashed ObjecdId of the AppUser and return the user's actual ObjectId from DB.
    /// </summary>
    /// <param name="userIdHashed"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Decrypted AppUser ObjedId</returns>
    public async Task<ObjectId?> GetActualUserId(string? userIdHashed, CancellationToken cancellationToken) =>
        string.IsNullOrEmpty(userIdHashed)
        ? null
        : await _collection.AsQueryable()
            .Where(appUser => appUser.IdentifierHash == userIdHashed)
            .Select(appUser => appUser.Id)
            .SingleOrDefaultAsync(cancellationToken);
}