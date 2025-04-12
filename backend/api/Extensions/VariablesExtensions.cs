namespace api.Extensions;

public static class AppVariablesExtensions
{
    public const string RecaptchaSecretKey = "RecaptchaSecretKey";

    // COLLECTIONS
    public const string CollectionUsers = "users";
    public const string CollectionRefreshTokens = "refresh-tokens";
    public const string CollectionFollows = "follows";
    public const string CollectionMessages = "messages";
    public const string CollectionOnlineTrackers = "online-trackers";
    public const string CollectionExceptionLogs = "exception-logs";

    // ROLES
    public const string RequiredAdminRole = "RequiredAdminRole";
    public const string RequiredModeratorRole = "RequiredModeratorRole";

    // RATE LIMIT
    public const string SlidingPolicy = "SlidingPolicy";
    public const string ConcurrentPolicy = "ConcurrentPolicy";

    // VERSION
    public static readonly string[] AppVersions = ["1", "1.0.2"];

    // APPROLE
    public static readonly AppRole[] AppRoles =
    [
        new() { Name = EnumExtensions.GetRoleStrValue(Roles.Admin) },
        new() { Name = EnumExtensions.GetRoleStrValue(Roles.Moderator) },
        new() { Name = EnumExtensions.GetRoleStrValue(Roles.Member) }
    ];
}