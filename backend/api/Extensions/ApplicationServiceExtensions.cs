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
            else
            {
                options.AddDefaultPolicy(policy => policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin() // production
                );
            }
        });

        services.AddScoped<LogUserActivity>(); // monitor/log userActivity

        #endregion Others

        return services;
    }
}