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

        #region Rate Limiting

        services.AddRateLimiter(
            options =>
            {
                options.AddPolicy(
                    AppVariablesExtensions.SlidingWindowPolicy, httpContext =>
                        RateLimitPartition.GetSlidingWindowLimiter(
                            httpContext.User.GetUserIdHashed(),
                            _ => new SlidingWindowRateLimiterOptions
                            {
                                PermitLimit = 4, // Up to 100 requests allowed
                                Window = TimeSpan.FromSeconds(30), // Sliding window of 5 minutes
                                SegmentsPerWindow = 5, // Smooth enforcement (1 segment per minute)
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = 10 // Allow up to 10 queued requests
                            }
                        )
                );

                options.AddPolicy(
                    AppVariablesExtensions.UserConcurrentPolicy, httpContext =>
                        RateLimitPartition.GetConcurrencyLimiter(
                            httpContext.User.GetUserIdHashed(),
                            _ => new ConcurrencyLimiterOptions
                            {
                                PermitLimit = 5, // Up to 5 requests allowed
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = 2 // Allow up to 2 queued requests        
                            }
                        )
                );
                
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests; // Too many requests
            }
        );

        #endregion

        #region Others

        services.AddScoped<LogUserActivity>(); // monitor/log userActivity

        services.AddHttpClient();

        services.AddSignalR(options => { options.AddFilter<SignalRExceptionHandler>(); });

        #endregion Others

        return services;
    }
}