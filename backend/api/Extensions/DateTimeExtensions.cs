namespace api.Extensions;

public static class DateTimeExtensions
{
    public static int CalculateAge(this DateOnly dob)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

        int age = today.Year - dob.Year;

        if (dob > today.AddYears(-age))
            age--;

        return age;
    }

    public static DateTime? GetTokenExpirationDate(string token)
    {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(token))
            return null;

        JwtSecurityToken? jwtToken = handler.ReadJwtToken(token);
        Claim? expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp");

        if (expClaim == null) return null;

        // Convert the expiration from Unix epoch to DateTime
        long expSeconds = long.Parse(expClaim.Value);
        return DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
    }
}