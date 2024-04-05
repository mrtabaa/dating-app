using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

#region Setup Configurations
// Register KeyVault for Azure
var keyVault = new Uri(builder.Configuration.GetSection("KeyVaultUrl").Value!);
var azureCredentials = new DefaultAzureCredential();
builder.Configuration.AddAzureKeyVault(keyVault, azureCredentials);

// Register User Secrets for Nginx
// builder.Configuration.AddUserSecrets<Program>();
#endregion

#region Add services to the container.
builder.Services.AddControllers();

// From customized ServiceExtensions (Extensions folder) for a claen maintained code //
builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddRepositoryServices();
#endregion

// Production: Set the URLs the app will listen on
// builder.WebHost.ConfigureKestrel(serverOptions =>
// {
//     if (builder.Environment.IsProduction())
//         serverOptions.ListenLocalhost(7100); // Listen for incoming HTTP connections on port 7100
// });

var port = Environment.GetEnvironmentVariable("WEBSITE_PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(int.Parse(port));
});

#region Configure the HTTP request pipeline.
var app = builder.Build();

// created a customized ExceptionMiddleware
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsProduction()) // https for production only
    app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
#endregion