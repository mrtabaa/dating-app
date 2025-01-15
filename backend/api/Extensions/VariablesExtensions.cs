namespace api.Extensions;

public static class AppVariablesExtensions
{
    public const string TokenKey = "TokenKey";
    public const string RecaptchaSecretKey = "RecaptchaSecretKey";

    public const string CollectionUsers = "users";
    public const string CollectionFollows = "follows";
    public const string CollectionMessages = "messages";
    public const string CollectionOnlineTrackers = "online-trackers";
    public const string CollectionExceptionLogs = "exception-logs";

    public const string RequiredAdminRole = "RequiredAdminRole";
    public const string RequiredModeratorRole = "RequiredModeratorRole";

    public static readonly string[] AppVersions = ["1", "1.0.2"];

    public static readonly AppRole[] Roles =
    [
        new() { Name = enums.Roles.Admin.ToString() },
        new() { Name = enums.Roles.Moderator.ToString() },
        new() { Name = enums.Roles.Member.ToString() }
    ];
}