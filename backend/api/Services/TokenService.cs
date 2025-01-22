namespace api.Services;

public class TokenService : ITokenService
{
    private readonly IMongoCollection<AppUser> _collection;
    private readonly SymmetricSecurityKey? _key; // set it as nullable by ? mark
    private readonly UserManager<AppUser> _userManager;

    public TokenService(
        IConfiguration config, IMongoClient client, IMyMongoDbSettings dbSettings, UserManager<AppUser> userManager
    )
    {
        IMongoDatabase dbName = client.GetDatabase(dbSettings.DatabaseName) ??
                                throw new ArgumentNullException(nameof(dbName));
        _collection = dbName.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);

        var tokenValue = config.GetValue<string>(AppVariablesExtensions.TokenKey);

        // throw exception if tokenValue is null
        _ = tokenValue ?? throw new ArgumentNullException(nameof(tokenValue), "tokenValue cannot be null");

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenValue));

        _userManager = userManager;
    }

    public async Task<string?> CreateToken(AppUser user, CancellationToken cancellationToken)
    {
        var jtiValue = Guid.NewGuid().ToString();

        string? identifierHash =
            await InsertEncryptedUserId(
                user.Id, jtiValue, cancellationToken
            ); // this securedId is stored in users collection to associate with the AppUser.

        if (!string.IsNullOrEmpty(identifierHash) && !string.IsNullOrEmpty(user.NormalizedUserName))
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, identifierHash),
                new(
                    JwtRegisteredClaimNames.Jti, jtiValue
                ) // session identifier or token ID, // TODO: store in db/cache to prevent multiple login sessions with one token. If already exists, reject new login.
            };

            // Get user's roles and add them all into claims
            IList<string> roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.ToUpper())));

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7), // Set expiration days
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        return null;
    }

    /// <summary>
    ///     Gets a userIdHashed of the AppUser and returns the user's actual ObjectId from DB.
    ///     It returns null if ObjectId or userIdHashed is invalid, Empty or null.
    /// </summary>
    /// <param name="userIdHashed"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Decrypted AppUser ObjectId OR null</returns>
    public async Task<ObjectId?> GetActualUserIdAsync(string? userIdHashed, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userIdHashed)) return null;

        ObjectId? userId = await _collection.AsQueryable()
            .Where(appUser => appUser.IdentifierHash == userIdHashed)
            .Select(appUser => appUser.Id)
            .SingleOrDefaultAsync(cancellationToken);

        return ValidationsExtension.ValidateObjectId(userId).IsSuccess ? userId : null;
    }

    private async Task<string?> InsertEncryptedUserId(
        ObjectId userId, string jtiValue, CancellationToken cancellationToken
    )
    {
        var newObjectId = ObjectId.GenerateNewId().ToString();

        string identifierHash = BCrypt.Net.BCrypt.HashPassword(newObjectId);

        UpdateDefinition<AppUser> updatedSecuredToken = Builders<AppUser>.Update
            .Set(appUser => appUser.IdentifierHash, identifierHash)
            .Set(appUser => appUser.JtiValue, jtiValue);

        UpdateResult updateResult = await _collection.UpdateOneAsync(
            appUser => appUser.Id == userId, updatedSecuredToken, null, cancellationToken
        );

        if (updateResult.ModifiedCount == 1)
            return identifierHash;

        return null;
    }
}