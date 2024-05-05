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
        services.Configure<MyMongoDbSettings>(config.GetSection(nameof(MyMongoDbSettings)));

        // get values
        services.AddSingleton<IMyMongoDbSettings>(serviceProvider =>
        serviceProvider.GetRequiredService<IOptions<MyMongoDbSettings>>().Value);

        // get connectionString to the db
        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            MyMongoDbSettings uri = serviceProvider.GetRequiredService<IOptions<MyMongoDbSettings>>().Value;

            return new MongoClient(uri.ConnectionString);
        });

        #endregion MongoDbSettings

        #region Azure storage
        string? storageConnectionString = config.GetValue<string>("StorageConnectionString"); // Azure blob

        if (!string.IsNullOrEmpty(storageConnectionString))
        {
            var blobServiceClient = new BlobServiceClient(storageConnectionString);

            // Add the BlobServiceClient to the services collection
            services.AddSingleton<BlobServiceClient>(blobServiceClient);
        };
        #endregion Azure storage

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
                    .WithOrigins("http://localhost:4300", "https://da-client-mr.azurewebsites.net", "https://hallboard.com/") // Nginx, Azure
                );
            }
        });

        services.AddScoped<LogUserActivity>(); // monitor/log userActivity

        #endregion Others

        return services;
    }
}