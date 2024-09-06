namespace api.SignalR.Models;

public record Connection(
    string ConnectionId,
    HashSet<string> GroupNames
);
