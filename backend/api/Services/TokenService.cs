namespace api.Services;

public class TokenService : ITokenService
{
    private readonly IMongoCollection<AppUser> _collectionAppUser;
    private readonly IMongoCollection<RefreshToken> _collectionRefreshTokens;
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<AppUser> _userManager;

    public TokenService(
        IConfiguration config, IMongoClient client, IMyMongoDbSettings dbSettings, UserManager<AppUser> userManager
    )
    {
        IMongoDatabase dbName = client.GetDatabase(dbSettings.DatabaseName) ??
                                throw new ArgumentNullException(nameof(dbName));

        _collectionAppUser = dbName.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);
        _collectionRefreshTokens = dbName.GetCollection<RefreshToken>(AppVariablesExtensions.CollectionRefreshTokens);

        _jwtSettings = config.GetSection(nameof(JwtSettings)).Get<JwtSettings>()
                       ?? throw new ArgumentNullException(nameof(JwtSettings));

        _userManager = userManager;
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

        ObjectId? userId = await _collectionAppUser.AsQueryable().
            Where(appUser => appUser.IdentifierHash == userIdHashed).
            Select(appUser => appUser.Id).SingleOrDefaultAsync(cancellationToken);

        return ValidationsExtension.ValidateObjectId(userId) ? userId : null;
    }

    public async Task<TokenDto> GenerateTokensAsync(
        RefreshTokenRequest refreshTokenRequest, AppUser appUser, CancellationToken cancellationToken
    )
    {
        string identifierHash = await StoreHashedUserId(
            appUser.Id, cancellationToken
        ); // this securedId is stored in user's collection to associate with the AppUser.

        return new TokenDto(
            await GenerateAccessTokenAsync(identifierHash, appUser),
            await GenerateRefreshTokenAsync(refreshTokenRequest, appUser.Id, cancellationToken)
        );
    }

    private async Task<string> GenerateAccessTokenAsync(string identifierHash, AppUser appUser)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, identifierHash)
        };

        // Get user's roles and add them all into claims. Admin/Moderator needs it
        IList<string> roles = await _userManager.GetRolesAsync(appUser);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.ToUpper())));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(10), // Short lifespan
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshTokenResponse> GenerateRefreshTokenAsync(
        RefreshTokenRequest refreshTokenRequest, ObjectId userId, CancellationToken cancellationToken
    )
    {
        var tokenValueRaw = Guid.CreateVersion7().ToString();

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            JtiValue = Guid.CreateVersion7().ToString(),
            TokenValueHashed = TokenHasher.HashWithSecret(tokenValueRaw, _jwtSettings.Key),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            SessionMetadata = refreshTokenRequest.SessionMetadata
        };

        await _collectionRefreshTokens.InsertOneAsync(refreshToken, null, cancellationToken);

        return new RefreshTokenResponse(
            tokenValueRaw,
            refreshToken.JtiValue,
            refreshToken.ExpiresAt
        );
    }

    private async Task<string> StoreHashedUserId(
        ObjectId userId, CancellationToken cancellationToken
    )
    {
        var identifierHash = Guid.CreateVersion7().ToString();

        UpdateDefinition<AppUser> updatedIdentifierHash = Builders<AppUser>.Update.
            Set(appUser => appUser.IdentifierHash, identifierHash);

        UpdateResult updateResult = await _collectionAppUser.UpdateOneAsync(
            appUser => appUser.Id == userId, updatedIdentifierHash, null, cancellationToken
        );

        return updateResult.ModifiedCount == 1
            ? identifierHash
            : throw new ApplicationException("Update IdentifierHash to DB failed.");
    }
}