#region Add services to the container.

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>(); // Register User Secrets

// AUTO-GENERATED CODES //
builder.Services.AddControllers();

// From customized ServiceExtensions (Extensions folder) for a claen maintained code //
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddRepositoryServices();
#endregion

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