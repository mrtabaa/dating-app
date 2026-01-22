WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

#region Add services to the container.

// From customized ServiceExtensions (Extensions folder) for a clean maintained code /
builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);
builder.Services.AddIdentityServices(builder.Configuration, builder.Environment);
builder.Services.AddThrottlingServices(builder.Configuration);
builder.Services.AddRepositoryServices();
builder.Services.AddHubServices();

#endregion

if (builder.Environment.IsProduction())
{
    // Production Azure: If no WEBSITE_PORT exists set to 8080
    string port = Environment.GetEnvironmentVariable("WEBSITE_PORT") ?? "8080";
    builder.WebHost.ConfigureKestrel(
        serverOptions =>
        {
            serverOptions.ListenAnyIP(int.Parse(port)); // TODO: set to an allowed ip only
        }
    );
}

#region Configure the HTTP request pipeline.

WebApplication app = builder.Build();

app.UseForwardedHeaders();

// created a customized ExceptionMiddleware
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<AuthRateLimitIdentifierMiddleware>();

if (app.Environment.IsProduction()) // https for production only
    app.UseHttpsRedirection();

// ✅ Add this logging middleware early
// app.Use(
//     async (context, next) =>
//     {
//         string? cookie = context.Request.Cookies["XSRF-TOKEN"];
//         string? header = context.Request.Headers["X-XSRF-TOKEN"].FirstOrDefault();
//
//         Console.WriteLine("🚀 Incoming request:");
//         Console.WriteLine($"  🔐 Cookie [XSRF-TOKEN]: {cookie}");
//         Console.WriteLine($"  📩 Header [X-XSRF-TOKEN]: {header}");
//
//         await next();
//     }
// );

app.UseCors();

app.UseRouting();

app.UseAuthentication(); // Validates and authenticates the request.
app.UseMiddleware<AuthenticatedUserIdGuardMiddleware>();

app.UseAuthorization(); // Populates HttpContext.User with claims from the token.

app.UseRateLimiter(); // Can now safely access the user's identity or claims for user-based rate limiting.

ControllerActionEndpointConventionBuilder controllerEndpoints = app.MapControllers();
controllerEndpoints.Add(
    endpointBuilder =>
    {
        if (endpointBuilder.Metadata.OfType<EnableRateLimitingAttribute>().Any())
            return;

        if (endpointBuilder.Metadata.OfType<AllowAnonymousAttribute>().Any())
            endpointBuilder.Metadata.Add(new EnableRateLimitingAttribute("public"));
        else if (endpointBuilder.Metadata.OfType<AuthorizeAttribute>().Any())
            endpointBuilder.Metadata.Add(new EnableRateLimitingAttribute("dashboard"));
    }
);

app.MapHubs();

app.Run();

#endregion
