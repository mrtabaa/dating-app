namespace api.Extensions;

public static class ClaimPrincipalExtensions
{
    public static string? GetUserIdHashed(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
