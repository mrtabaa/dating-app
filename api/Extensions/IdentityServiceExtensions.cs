namespace api.Extensions;

public static class IdentityServiceExtensions {
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config) {
        string? tokenValue = config[ConstStringValues.TokenKey];

        _ = tokenValue ?? throw new ArgumentNullException("tokenValue cannot be null", nameof(tokenValue));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenValue)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

        return services;
    }
}