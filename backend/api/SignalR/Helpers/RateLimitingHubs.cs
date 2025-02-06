using System.Collections.Concurrent;

namespace api.SignalR.Helpers;

public static class RateLimitingHubs
{
    private const int MessageLimit = 30;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> UserRateLimits = new();
    private static readonly ConcurrentDictionary<string, List<DateTime>> UserMessageTimestamps = new();
    private static readonly TimeSpan SlidingWindow = TimeSpan.FromMinutes(1);

    public static async Task<SemaphoreSlim?> RateLimitConcurrentAsync(
        string userIdHashed, IHubCallerClients clients, CancellationToken cancellationToken
    )
    {
        // Use a SemaphoreSlim per user to simulate rate limiting
        SemaphoreSlim
            semaphore = UserRateLimits.GetOrAdd(userIdHashed, _ => new SemaphoreSlim(1, 1)); // 1 message allowed


        if (!await semaphore.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken))
        {
            await clients.Caller.SendAsync(
                SignalRMessages.SendingError, "Too many messages, please slow down.", cancellationToken
            );
            return null;
        }

        return semaphore;
    }

    public static bool RateLimitSliding(
        string userIdHashed, IHubCallerClients clients, CancellationToken cancellationToken
    )
    {
        // Ensure the timestamp list exists for the user
        if (!UserMessageTimestamps.TryGetValue(userIdHashed, out List<DateTime>? timestamps))
        {
            timestamps = new List<DateTime>();
            UserMessageTimestamps[userIdHashed] = timestamps;
        }

        lock (timestamps)
        {
            // Remove timestamps outside the sliding window
            DateTime windowStart = DateTime.UtcNow.Subtract(SlidingWindow);
            timestamps.RemoveAll(ts => ts < windowStart);

            if (timestamps.Count >= MessageLimit)
            {
                clients.Caller.SendAsync(
                    SignalRMessages.SendingError, "Too many messages, please slow down.", cancellationToken
                );
                return true;
            }

            // Add the current timestamp
            timestamps.Add(DateTime.UtcNow);
        }

        return false;
    }
}