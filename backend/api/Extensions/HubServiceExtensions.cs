namespace api.Extensions;

public static class HubServiceExtensions
{
    public static IServiceCollection AddHubServices(this IServiceCollection services)
    {
        services.AddSingleton<IPresenceTrackerService, PresenceTrackerService>();
        services.AddScoped<IMessageService, MessageService>();

        return services;
    }
}