#region Add services to the container.

var builder = WebApplication.CreateBuilder(args);

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

// app.UseHttpsRedirection(); // disable https for development

app.UseStaticFiles();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
#endregion