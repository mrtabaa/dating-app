using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;

namespace api.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
        #region Token
        string? tokenValue = config.GetValue<string>(AppVariablesExtensions.TokenKey);

        if (tokenValue is not null)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenValue)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true
                        // ValidIssuer = "https://localhost:5101", // TODO Apply these
                        // ValidAudience = "https://localhost:5101",

                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context => HandleOnMessageReceived(context) // see the private method bellow
                    };
                });
        }
        #endregion

        #region MongoIdentity & Role
        var mongoDbSettings = config.GetSection(nameof(MyMongoDbSettings)).Get<MyMongoDbSettings>();

        if (mongoDbSettings is not null)
        {
            var mongoDbIdentityConfig = new MongoDbIdentityConfiguration
            {
                MongoDbSettings = new MongoDbSettings
                {
                    ConnectionString = mongoDbSettings.ConnectionString,
                    DatabaseName = mongoDbSettings.DatabaseName
                },
                IdentityOptionsAction = options =>
                {
                    // Unique email
                    options.User.RequireUniqueEmail = true;

                    // Password requirements
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireLowercase = false;

                    // Lockout
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                }
            };

            services.ConfigureMongoDbIdentity<AppUser, AppRole, ObjectId>(mongoDbIdentityConfig)
            .AddUserManager<UserManager<AppUser>>()
            .AddSignInManager<SignInManager<AppUser>>()
            .AddRoleManager<RoleManager<AppRole>>()
            .AddDefaultTokenProviders();
        }
        #endregion

        #region Policy
        services.AddAuthorizationBuilder()
            .AddPolicy("RequiredAdminRole", policy => policy.RequireRole(Roles.admin.ToString()))
            .AddPolicy("ModeratePhotoRole", policy => policy.RequireRole(Roles.admin.ToString(), Roles.moderator.ToString()));
        #endregion

        return services;
    }

    /// <summary>
    ///  Enable/Customize the JwtBearer authentication middleware to extract the JWT token from the query string for requests
    ///  made to SignalR hubs. This is particularly useful in scenarios where the token cannot be sent in the 
    ///  Authorization header (which is the standard way of sending tokens) due to WebSocket or other constraints.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static Task HandleOnMessageReceived(MessageReceivedContext context)
    {
        const string AccessTokenQueryParameter = "access_token";
        const string HubsPathSegment = "/hubs";

        var accessToken = context.Request.Query[AccessTokenQueryParameter];
        PathString path = context.HttpContext.Request.Path;

        if (!string.IsNullOrWhiteSpace(accessToken) && path.StartsWithSegments(HubsPathSegment))
            context.Token = accessToken;

        return Task.CompletedTask;
    }
}