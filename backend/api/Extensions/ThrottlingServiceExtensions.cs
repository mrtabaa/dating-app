using Microsoft.AspNetCore.HttpOverrides;

namespace api.Extensions;

public static class ThrottlingServiceExtensions
{
    public static IServiceCollection AddThrottlingServices(
        this IServiceCollection services, IConfiguration config
    )
    {
        SetThrottling(services, config);

        return services;
    }

    private static void SetThrottling(IServiceCollection services, IConfiguration config)
    {
        SetForwardedHeaders(services, config);

        services.AddRateLimiter(options =>
        {
            // Set errors on rejections
            SetOnRejected(options);

            // Get keys
            static string GetIpKey(HttpContext httpContext)
            {
                string? ip = httpContext.Connection.RemoteIpAddress?.ToString();
                // NOTE: Make sure Forwarded Headers are configured if behind a proxy.
                return ip is not null ? $"ip:{ip}" : "ip:unknown";
            }
            static string GetPartitionKey(HttpContext httpContext)
            {
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    string? userIdHashed = httpContext.User.GetUserIdHashed();
                    if (!string.IsNullOrWhiteSpace(userIdHashed))
                        return $"u:{userIdHashed}";
                }

                return GetIpKey(httpContext);
            }
            static string GetAuthPartitionKey(HttpContext httpContext)
            {
                string ipKey = GetIpKey(httpContext);

                return httpContext.Items.TryGetValue(AuthRateLimitIdentifierMiddleware.AuthIdentifierItemKey, out var value)
                    && value is string idHash
                    && !string.IsNullOrWhiteSpace(idHash)
                    ? $"{ipKey}|id:{idHash}"
                    : ipKey;
            }

            static int GetConcurrencyLimit(HttpContext httpContext)
            {
                Endpoint? endpoint = httpContext.GetEndpoint();
                EnableRateLimitingAttribute? rateLimit =
                    endpoint?.Metadata.GetMetadata<EnableRateLimitingAttribute>();

                if (rateLimit is not null)
                {
                    return rateLimit.PolicyName switch
                    {
                        "public" => 4,
                        "dashboard" => 10,
                        "auth" => 2,
                        "hubs" => 3,
                        _ => 8
                    };
                }

                if (endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() is not null)
                    return 4;

                if (endpoint?.Metadata.GetMetadata<AuthorizeAttribute>() is not null)
                    return 10;

                return 8;
            }

            options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                // Rate-Limiting / General API limiter: per user or per IP.
                PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    string key = GetPartitionKey(httpContext);

                    return RateLimitPartition.GetTokenBucketLimiter(
                        key,
                        _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 300,
                            TokensPerPeriod = 300,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                            AutoReplenishment = true,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0 // 0: avoid memory spikes, fail fast & protect server VS absorb pressure & increase latency
                        });
                }),

                // Throttling: Concurrency limiter to reduce resource spikes.
                PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    string key = GetPartitionKey(httpContext);

                    return RateLimitPartition.GetConcurrencyLimiter(
                        key,
                        _ => new ConcurrencyLimiterOptions
                        {
                            PermitLimit = GetConcurrencyLimit(httpContext),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                })
            );

            // Stricter limiter for anonymous auth endpoints (login/register/verify/reset).
            options.AddPolicy("auth", httpContext =>
            {
                string key = GetAuthPartitionKey(httpContext);

                return RateLimitPartition.GetSlidingWindowLimiter(
                    key,
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(5),
                        SegmentsPerWindow = 5,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            options.AddPolicy("public", httpContext =>
            {
                string key = GetIpKey(httpContext);
                return RateLimitPartition.GetTokenBucketLimiter(
                    key,
                    _ => new TokenBucketRateLimiterOptions
                    {
                        // This allows small bursts of 20 while keeping 2/sec average.
                        TokenLimit = 20,
                        TokensPerPeriod = 2,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                        AutoReplenishment = true,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            options.AddPolicy("dashboard", httpContext =>
            {
                string key = GetPartitionKey(httpContext);
                return RateLimitPartition.GetTokenBucketLimiter(
                    key,
                    _ => new TokenBucketRateLimiterOptions
                    {
                        // (600/min)
                        TokenLimit = 50,
                        TokensPerPeriod = 10,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                        AutoReplenishment = true,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            options.AddPolicy("hubs", httpContext =>
            {
                string key = GetPartitionKey(httpContext); // e.g. u:{userIdHashed} or ip:{ip}

                return RateLimitPartition.GetSlidingWindowLimiter(
                    key,
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 10,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });
    }

    private static void SetOnRejected(RateLimiterOptions options)
    {
        options.OnRejected = async (context, token) =>
        {
            if (context.HttpContext.Response.HasStarted)
                return;

            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

            ProblemDetails problemDetails = new()
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too many requests.",
                Detail = "Please slow down and try again."
            };

            await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, token);
        };
    }

    private static void SetForwardedHeaders(IServiceCollection services, IConfiguration config)
    {
        services.Configure<ForwardedHeadersOptions>(
            options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.ForwardLimit = 1;

                IConfigurationSection forwardedSection = config.GetSection("ForwardedHeaders");
                string[]? knownProxies = forwardedSection.GetSection("KnownProxies").Get<string[]>();
                string[]? knownNetworks = forwardedSection.GetSection("KnownNetworks").Get<string[]>();

                if ((knownProxies?.Length ?? 0) > 0 || (knownNetworks?.Length ?? 0) > 0)
                {
                    options.KnownNetworks.Clear();
                    options.KnownProxies.Clear();

                    if (knownProxies is not null)
                    {
                        foreach (string proxy in knownProxies)
                        {
                            if (!IPAddress.TryParse(proxy, out IPAddress? parsedProxy))
                                throw new InvalidOperationException(
                                    $"ForwardedHeaders: KnownProxies contains invalid IP '{proxy}'."
                                );

                            options.KnownProxies.Add(parsedProxy);
                        }
                    }

                    if (knownNetworks is not null)
                    {
                        foreach (string cidr in knownNetworks)
                        {
                            string[] parts = cidr.Split('/');
                            if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out IPAddress? networkIp))
                                throw new InvalidOperationException(
                                    $"ForwardedHeaders: KnownNetworks contains invalid CIDR '{cidr}'."
                                );

                            if (!int.TryParse(parts[1], out int prefixLength))
                                throw new InvalidOperationException(
                                    $"ForwardedHeaders: KnownNetworks contains invalid CIDR prefix '{cidr}'."
                                );

                            bool isV4 = networkIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
                            int maxPrefix = isV4 ? 32 : 128;
                            if (prefixLength < 0 || prefixLength > maxPrefix)
                                throw new InvalidOperationException(
                                    $"ForwardedHeaders: KnownNetworks CIDR prefix out of range '{cidr}'."
                                );

                            options.KnownNetworks.Add(
                                new Microsoft.AspNetCore.HttpOverrides.IPNetwork(networkIp, prefixLength)
                            );
                        }
                    }
                }
            }
        );
    }
}
