namespace api.Extensions;

public static class EnumExtensions
{
    public static string GetRoleStrValue(Roles role) =>
        role.ToString().ToUpper();
}