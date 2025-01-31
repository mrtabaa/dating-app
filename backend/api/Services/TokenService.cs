namespace api.Services;

public class TokenService : ITokenService
{
    private readonly IMongoCollection<AppUser> _collection;
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<AppUser> _userManager;

    public TokenService(
        IConfiguration config, IMongoClient client, IMyMongoDbSettings dbSettings, UserManager<AppUser> userManager
    )
    {
        IMongoDatabase dbName = client.GetDatabase(dbSettings.DatabaseName) ??
                                throw new ArgumentNullException(nameof(dbName));

        _collection = dbName.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);

        _jwtSettings = config.GetSection(nameof(JwtSettings)).Get<JwtSettings>()
                       ?? throw new ArgumentNullException(nameof(JwtSettings));

        _userManager = userManager;
    }

    public async Task<TokenDto> GenerateTokensAsync(AppUser appUser, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(appUser.NormalizedUserName))
        {
            throw new ArgumentNullException(
                nameof(appUser.NormalizedUserName), "NormalizedUserName cannot be null."
            );
        }

        var jtiValue = Guid.CreateVersion7().ToString();

        string identifierHash = await InsertHashedUserId(
            appUser.Id, jtiValue, cancellationToken
        ); // this securedId is stored in users collection to associate with the AppUser.

        return new TokenDto(
            await GenerateAccessTokenAsync(appUser, identifierHash, jtiValue),
            await GenerateRefreshTokenAsync(appUser.Id, cancellationToken)
        );
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

    private async Task<string> GenerateAccessTokenAsync(AppUser appUser, string identifierHash, string jtiValue)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, identifierHash),
            new(JwtRegisteredClaimNames.Jti, jtiValue)
        };

        // Get user's roles and add them all into claims
        IList<string> roles = await _userManager.GetRolesAsync(appUser);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.ToUpper())));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(15), // Short lifespan
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshTokenDto> GenerateRefreshTokenAsync(ObjectId userId, CancellationToken cancellationToken)
    {
        var refreshToken = Guid.CreateVersion7().ToString();
        DateTime refreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Long lifespan

        UpdateDefinition<AppUser> updateDef = Builders<AppUser>.Update.
            Set(appUser => appUser.RefreshToken, refreshToken).Set(
                appUser => appUser.RefreshTokenExpiryTime, refreshTokenExpiryTime
            );

        UpdateResult? updateResult = await _collection.UpdateOneAsync(
            appUser => appUser.Id == userId, updateDef, null, cancellationToken
        );

        if (updateResult.ModifiedCount < 1)
            throw new ArgumentException("Failed to generate refresh token.");

        return new RefreshTokenDto(refreshToken, refreshTokenExpiryTime);
    }

    private async Task<string> InsertHashedUserId(
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

        return updateResult.ModifiedCount == 1
            ? identifierHash
            : throw new ApplicationException("Update IdentifierHash to DB failed.");
    }
}