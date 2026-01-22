using System.Security.Cryptography;

namespace api.Middleware;

public sealed class AuthRateLimitIdentifierMiddleware(RequestDelegate next, IConfiguration config)
{
    public const string AuthIdentifierItemKey = "RateLimiting.AuthIdentifierHash";
    private const int MaxBodyBytesToInspect = 8 * 1024;

    private static readonly HashSet<string> AllowedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/account/login",
        "/api/account/register",
        "/api/account/verify",
        "/api/account/resend-verify-code",
        "/api/account/request-reset-password",
        "/api/account/reset-password"
    };

    private readonly RequestDelegate _next = next;

    // Precompute once
    private readonly byte[] _authIdKeyBytes = Encoding.UTF8.GetBytes(
        config["RateLimiting:AuthIdentifierKey"]
        ?? throw new InvalidOperationException("Missing RateLimiting:AuthIdentifierKey configuration.")
    );

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method))
        {
            await _next(context);
            return;
        }

        string path = (context.Request.Path.Value ?? string.Empty).TrimEnd('/');
        if (!AllowedPaths.Contains(path))
        {
            await _next(context);
            return;
        }

        if (context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) != true)
        {
            await _next(context);
            return;
        }

        if (context.Request.ContentLength is long len && len > MaxBodyBytesToInspect)
        {
            await _next(context);
            return;
        }

        if (context.Request.ContentLength == 0)
        {
            await _next(context);
            return;
        }

        context.Request.EnableBuffering();

        string body;
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, false, leaveOpen: true))
        {
            char[] buffer = new char[MaxBodyBytesToInspect];
            int read = await reader.ReadBlockAsync(buffer, 0, buffer.Length);
            body = read > 0 ? new string(buffer, 0, read) : string.Empty;
            context.Request.Body.Position = 0;
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            await _next(context);
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                await _next(context);
                return;
            }

            string? identifier =
                TryGetString(doc.RootElement, "email") ??
                TryGetString(doc.RootElement, "emailUsername") ??
                TryGetString(doc.RootElement, "userName") ??
                TryGetString(doc.RootElement, "username");

            if (!string.IsNullOrWhiteSpace(identifier))
            {
                string normalized = identifier.Trim().ToLowerInvariant();
                string hash = HashIdentifier(normalized);
                context.Items[AuthIdentifierItemKey] = hash;
            }
        }
        catch (JsonException)
        {
            // ignore; fall back to IP-only partitioning
        }

        await _next(context);
    }

    private static string? TryGetString(JsonElement obj, string propertyName)
    {
        if (!obj.TryGetProperty(propertyName, out var prop)) return null;
        return prop.ValueKind == JsonValueKind.String ? prop.GetString() : null;
    }

    private string HashIdentifier(string input)
    {
        byte[] data = Encoding.UTF8.GetBytes(input);
        byte[] hash = HMACSHA256.HashData(_authIdKeyBytes, data);
        return Convert.ToHexString(hash);
    }
}
