namespace api.Extensions;

public static class ApplicationServiceExtensions {

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config) {

        #region MongoDbSettings            
        services.Configure<MongoDbSettings>(config.GetSection(nameof(MongoDbSettings)));

        services.AddSingleton<IMongoDbSettings>(sp =>
        sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);

        services.AddSingleton<IMongoClient>(serviceProvider => {
            var uri = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(uri.ConnectionString);
        });
        #endregion MongoDbSettings

        #region Others
        // with ssl 
        services.AddCors(options => {
            options.AddDefaultPolicy(policy => policy.AllowAnyHeader()
                .AllowAnyMethod().WithOrigins("https://localhost:4200"));
        });

        #endregion Others

        return services;
    }
}