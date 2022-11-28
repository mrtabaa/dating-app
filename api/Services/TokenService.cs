namespace api.Services;
public class TokenService : ITokenService {
    private readonly SymmetricSecurityKey? _key; // set it as nullable by ? mark
    const string TokenKey = "TokenKey";

    public TokenService(IConfiguration config) {
        string? tokenValue = config[TokenKey];

        if (tokenValue != null) {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenValue));
        }
    }

    public string CreateToken(AppUser user) {
        _ = _key ?? throw new ArgumentNullException("_key cannot be null", nameof(_key));

        var claims = new List<Claim> {
            new Claim(JwtRegisteredClaimNames.NameId, user.Id!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Name, user.Name!),
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}