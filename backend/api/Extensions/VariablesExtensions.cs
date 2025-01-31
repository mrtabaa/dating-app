namespace api.Extensions;

public static class AppVariablesExtensions
{
    public const string RecaptchaSecretKey = "RecaptchaSecretKey";

    public const string CollectionUsers = "users";
    public const string CollectionFollows = "follows";
    public const string CollectionMessages = "messages";
    public const string CollectionOnlineTrackers = "online-trackers";
    public const string CollectionExceptionLogs = "exception-logs";

    public const string RequiredAdminRole = "RequiredAdminRole";
    public const string RequiredModeratorRole = "RequiredModeratorRole";
    public static readonly string[] AppVersions = ["1", "1.0.2"];

    public static readonly AppRole[] AppRoles =
    [
        new() { Name = EnumExtensions.GetRoleStrValue(Roles.Admin) },
        new() { Name = EnumExtensions.GetRoleStrValue(Roles.Moderator) },
        new() { Name = EnumExtensions.GetRoleStrValue(Roles.Member) }
    ];
}