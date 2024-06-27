using AspNetCore.Identity.MongoDbCore.Infrastructure;

namespace api.Extensions;

public static class ApplicationServiceExtensions
{

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, IConfiguration config, IWebHostEnvironment env
    )
    {
        #region MongoDbSettings
        ///// get values from this file: appsettings.Development.json /////
        // Bind MyMongoDbSettings from appsettings.json
        var mongoDbSettingsSection = config.GetSection(nameof(MyMongoDbSettings));
        services.Configure<MyMongoDbSettings>(mongoDbSettingsSection);

        // Override ConnectionString and DatabaseName if environment variables are set
        var mongoDbSettings = new MyMongoDbSettings();
        mongoDbSettingsSection.Bind(mongoDbSettings);

        var envConnectionString = Environment.GetEnvironmentVariable("MONGODB_URI");
        if (!string.IsNullOrEmpty(envConnectionString))
        {
            mongoDbSettings.ConnectionString = envConnectionString;
        }

        var envDatabaseName = Environment.GetEnvironmentVariable("MONGODB_DBNAME");
        if (!string.IsNullOrEmpty(envDatabaseName))
        {
            mongoDbSettings.DatabaseName = envDatabaseName;
        }

        // Register MyMongoDbSettings instance
        services.AddSingleton<IMyMongoDbSettings>(sp => mongoDbSettings);

        // Register MongoClient
        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IMyMongoDbSettings>();
            return new MongoClient(settings.ConnectionString);
        });

        #endregion MongoDbSettings

        // #region Azure storage
        // string? storageConnectionString = config.GetValue<string>("StorageConnectionString"); // Azure blob

        // if (!string.IsNullOrEmpty(storageConnectionString))
        // {
        //     var blobServiceClient = new BlobServiceClient(storageConnectionString);

        //     // Add the BlobServiceClient to the services collection
        //     services.AddSingleton<BlobServiceClient>(blobServiceClient);
        // };
        // #endregion Azure storage

        #region Others
        services.AddCors(options =>
        {
            if (env.IsDevelopment())
            {
                options.AddDefaultPolicy(policy => policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins("http://localhost:4300")
                );
            }
            else if (env.IsProduction())
            {
                options.AddDefaultPolicy(policy => policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins("https://hallboard.com") // Nginx, AWS
                );
            }
        });

        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddDebug();
            // Add other log providers as needed
        });

        services.AddHealthChecks();

        services.AddScoped<LogUserActivity>(); // monitor/log userActivity

        #endregion Others

        return services;
    }
}