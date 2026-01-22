namespace api.Extensions;

public static class HubMappingExtensions
{
    public static void MapHubs(this WebApplication app)
    {
        app.MapHub<PresenceHub>("hubs/presence").RequireRateLimiting("hubs");
        app.MapHub<MessageHub>("hubs/message").RequireRateLimiting("hubs");
    }
}
