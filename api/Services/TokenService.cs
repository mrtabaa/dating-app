namespace api.Services;
public class TokenService : ITokenService
{
    //TODO: A token authorizes all users. Should each user work with one token only?

    private readonly SymmetricSecurityKey? _key; // set it as nullable by ? mark

    public TokenService(IConfiguration config)
    {
        string? tokenValue = config[AppVariablesExtensions.TokenKey];

        // throw exception if tokenValue is null
        _ = tokenValue ?? throw new ArgumentNullException("tokenValue cannot be null", nameof(tokenValue));

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenValue!));
    }

    public string CreateToken(AppUser user)
    {
        _ = _key ?? throw new ArgumentNullException("_key cannot be null", nameof(_key));

        var claims = new List<Claim> {
            new Claim(JwtRegisteredClaimNames.NameId, user.Id!)
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7), // Set expiration days
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}