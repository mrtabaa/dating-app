namespace api.Extensions;

public static class ClaimPrincipalExtensions
{
    public static string? GetUserIdHashed(this ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}