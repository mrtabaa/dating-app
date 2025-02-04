WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

#region Add services to the container.

builder.Services.AddControllers();

// From customized ServiceExtensions (Extensions folder) for a clean maintained code /
builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);
builder.Services.AddIdentityServices(builder.Configuration);
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

app.UseRateLimiter();

// created a customized ExceptionMiddleware
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsProduction()) // https for production only
    app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHubs();

app.Run();

#endregion