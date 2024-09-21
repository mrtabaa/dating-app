namespace api.SignalR.Models;

public class ConnectionMessage
{
    public string ConnectionId { get; set; } = string.Empty;
    public HashSet<string> GroupNames { get; set; } = [];
}
