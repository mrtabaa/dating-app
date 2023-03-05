
namespace api.Extensions;

public static class ApplicationServiceExtensions
{

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {

        #region MongoDbSettings
        ///// get values from this file: appsettings.Development.json /////
        // get section
        services.Configure<MongoDbSettings>(config.GetSection(nameof(MongoDbSettings)));

        // get values
        services.AddSingleton<IMongoDbSettings>(serviceProvider =>
        serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value);

        // get connectionString to the db
        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            MongoDbSettings uri = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;

            return new MongoClient(uri.ConnectionString);
        });

        #endregion MongoDbSettings

        #region Others
        // with ssl 
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy => policy.AllowAnyHeader()
                .AllowAnyMethod().WithOrigins("https://localhost:4200"));
        });

        #endregion Others

        return services;
    }
}