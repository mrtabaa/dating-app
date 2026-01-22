namespace api.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, IConfiguration config, IWebHostEnvironment env
    )
    {
        #region MongoDbSettings

        ///// get values from this file: appsettings.Development.json /////
        // get the section
        services.Configure<MyMongoDbSettings>(config.GetSection(nameof(MyMongoDbSettings)));

        // get values
        services.AddSingleton<IMyMongoDbSettings>(
            serviceProvider => serviceProvider.GetRequiredService<IOptions<MyMongoDbSettings>>().Value
        );

        // get connectionString to the db
        services.AddSingleton<IMongoClient>(
            serviceProvider =>
            {
                MyMongoDbSettings myMongoDbSettings = serviceProvider.GetRequiredService<IOptions<MyMongoDbSettings>>().
                    Value;

                return new MongoClient(myMongoDbSettings.ConnectionString);
            }
        );

        #endregion MongoDbSettings

        #region Azure storage

        var storageConnectionString = config.GetValue<string>("StorageConnectionString"); // Azure blob

        if (!string.IsNullOrEmpty(storageConnectionString))
        {
            var blobServiceClient = new BlobServiceClient(storageConnectionString);

            // Add the BlobServiceClient to the services collection
            services.AddSingleton(blobServiceClient);
        }

        #endregion Azure storage

        #region CORS

        services.AddCors(
            options =>
            {
                if (env.IsDevelopment())
                {
                    options.AddDefaultPolicy(
                        policy => policy.WithOrigins("http://localhost:4300").AllowAnyHeader().AllowAnyMethod().
                            AllowCredentials()
                    );
                }
                else
                {
                    options.AddDefaultPolicy(
                        policy => policy.WithOrigins(
                            "https://da-client-mr.azurewebsites.net",
                            "https://hallboard.com",
                            "https://www.hallboard.com"
                        ).AllowAnyHeader().AllowAnyMethod().AllowCredentials()
                    );
                }
            }
        );

        #endregion CORS

        #region Others

        services.AddScoped<LogUserActivity>(); // monitor/log userActivity

        services.AddHttpClient();

        services.AddSignalR(
            options =>
            {
                options.AddFilter<SignalRExceptionHandler>();
                options.AddFilter<CsrfHubFilter>();
            }
        );

        #endregion Others

        return services;
    }
}
