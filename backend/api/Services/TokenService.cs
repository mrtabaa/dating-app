namespace api.Services;

public class TokenService : ITokenService
{
    private readonly IMongoCollection<AppUser> _collection;

    // private readonly SymmetricSecurityKey? _key; // set it as nullable by ? mark
    private readonly IConfiguration _config;
    private readonly UserManager<AppUser> _userManager;

    public TokenService(
        IConfiguration config, IMongoClient client, IMyMongoDbSettings dbSettings, UserManager<AppUser> userManager
    )
    {
        IMongoDatabase dbName = client.GetDatabase(dbSettings.DatabaseName) ??
                                throw new ArgumentNullException(nameof(dbName));
        _collection = dbName.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);

        string tokenValue = config.GetValue<string>(AppVariablesExtensions.TokenKey)
                            ?? throw new ArgumentNullException(nameof(tokenValue), "tokenValue cannot be null");
        ;

        // _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenValue));

        _config = config;
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

        ObjectId? userId = await _collection.AsQueryable().Where(appUser => appUser.IdentifierHash == userIdHashed).
            Select(appUser => appUser.Id).SingleOrDefaultAsync(cancellationToken);

        return ValidationsExtension.ValidateObjectId(userId).IsSuccess ? userId : null;
    }

    public async Task<string> GenerateAccessTokenAsync(AppUser appUser, CancellationToken cancellationToken)
    {
        var jtiValue = Guid.CreateVersion7().ToString();

        string? identifierHash = await InsertHashedUserId(
            appUser.Id, jtiValue, cancellationToken
        ); // this securedId is stored in users collection to associate with the AppUser.

        if (string.IsNullOrEmpty(identifierHash) || string.IsNullOrEmpty(appUser.NormalizedUserName))
            throw new ArgumentNullException(
                nameof(identifierHash), "identifierHash or normalizedUserName cannot be null."
            );

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, identifierHash),
            new(JwtRegisteredClaimNames.Jti, jtiValue)
        };

        // Get user's roles and add them all into claims
        IList<string> roles = await _userManager.GetRolesAsync(appUser);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.ToUpper())));

        string jwtKey = _config["Jwt:Key"]
                        ?? throw new ArgumentNullException(null, "Jwt:Key");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            _config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddMinutes(15), // Short lifespan
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(AppUser appUser)
    {
        appUser.RefreshToken = Guid.CreateVersion7().ToString();
        appUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Long lifespan

        IdentityResult result = await _userManager.UpdateAsync(appUser);

        if (!result.Succeeded)
            throw new ArgumentException("Failed to generate refresh token.");

        return appUser.RefreshToken;
    }

    private async Task<string?> InsertHashedUserId(
        ObjectId userId, string jtiValue, CancellationToken cancellationToken
    )
    {
        var newObjectId = ObjectId.GenerateNewId().ToString();

        string identifierHash = BCrypt.Net.BCrypt.HashPassword(newObjectId);

        UpdateDefinition<AppUser> updatedSecuredToken = Builders<AppUser>.Update.
            Set(appUser => appUser.IdentifierHash, identifierHash).Set(appUser => appUser.JtiValue, jtiValue);

        UpdateResult updateResult = await _collection.UpdateOneAsync(
            appUser => appUser.Id == userId, updatedSecuredToken, null, cancellationToken
        );

        if (updateResult.ModifiedCount == 1)
            return identifierHash;

        return null;
    }
}