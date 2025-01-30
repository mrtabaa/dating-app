using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;

namespace api.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
        #region Token

        services.AddAuthentication(
            options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }
        ).AddJwtBearer(
            options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    RoleClaimType = ClaimTypes.Role, // Ensure it matches how roles are stored in the token
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"])),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = config["Jwt:Issuer"], // TODO: Convert them to options
                    ValidAudience = config["Jwt:Audience"]
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["access-token"];
                        return Task.CompletedTask;
                    }
                };
            }
        );

        #endregion

        #region MongoIdentity & Role

        var mongoDbSettings = config.GetSection(nameof(MyMongoDbSettings)).Get<MyMongoDbSettings>();
        string tokenValue = config.GetValue<string>(AppVariablesExtensions.TokenKey)
                            ?? throw new ArgumentNullException(
                                nameof(AppVariablesExtensions.TokenKey), "Token cannot be null here."
                            );

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

                    // Verify confirmed email. No account confirmation
                    options.SignIn.RequireConfirmedEmail = true;
                    options.SignIn.RequireConfirmedAccount = false;
                    options.Tokens.EmailConfirmationTokenProvider =
                        TokenOptions.DefaultEmailProvider; // shorten code to 6 digits

                    // Token handling
                    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;

                    // Password requirements
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 8;

                    // Lockout
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                }
            };

            services.ConfigureMongoDbIdentity<AppUser, AppRole, ObjectId>(mongoDbIdentityConfig).
                AddUserManager<UserManager<AppUser>>().AddSignInManager<SignInManager<AppUser>>().
                AddRoleManager<RoleManager<AppRole>>().AddDefaultTokenProviders();
        }

        #endregion

        #region Policy

        services.AddAuthorizationBuilder().
            AddPolicy(
                AppVariablesExtensions.RequiredAdminRole, policy => policy.RequireRole(Roles.Admin.ToString().ToUpper())
            ).AddPolicy(
                AppVariablesExtensions.RequiredModeratorRole,
                policy => policy.RequireRole(Roles.Admin.ToString().ToUpper(), Roles.Moderator.ToString().ToUpper())
            );

        #endregion

        return services;
    }
}