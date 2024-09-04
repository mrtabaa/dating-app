namespace api.Extensions;

public static class HubMappingExtensions
{
    public static void MapHubs(this WebApplication app)
    {
        app.MapHub<PresenceHub>("hubs/presence");
        app.MapHub<MessageHub>("hubs/message");
    }
}
