namespace api.Extensions;

public static class ClaimPrincipalExtensions
{
    // public static string? GetUserId(this ClaimsPrincipal user)
    // {
    //     return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    // }
    
    public static string? GetUserEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value; 
    }
}
