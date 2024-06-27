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
        // get section
        MongoDbSettings mongoDbSettings = new();

        config.GetSection(nameof(MyMongoDbSettings)).Bind(mongoDbSettings);

        // Override ConnectionString if MONGODB_URI environment variable is set
        var envConnectionString = Environment.GetEnvironmentVariable("MONGODB_URI");
        if (!string.IsNullOrEmpty(envConnectionString))
        {
            mongoDbSettings.ConnectionString = envConnectionString;
        }

        // get values
        services.AddSingleton<IMyMongoDbSettings>(serviceProvider =>
            serviceProvider.GetRequiredService<IOptions<MyMongoDbSettings>>().Value
        );

        // get connectionString to the db
        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            MyMongoDbSettings settings = serviceProvider.GetRequiredService<IOptions<MyMongoDbSettings>>().Value;

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