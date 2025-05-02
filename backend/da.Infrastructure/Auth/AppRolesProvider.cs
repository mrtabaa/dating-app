using da.Domain.Enums;
using da.Infrastructure.Mongo.Models;

namespace da.Infrastructure.Auth;

public static class AppRolesProvider
{
    public static readonly MongoAppRole[] AppRoles =
    [
        new() { Name = GetRoleStrValue(Roles.Admin) },
        new() { Name = GetRoleStrValue(Roles.Moderator) },
        new() { Name = GetRoleStrValue(Roles.Member) }
    ];

    public static string GetRoleStrValue(Roles role) =>
        role.ToString().ToUpper();
}