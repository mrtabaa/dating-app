namespace api.Extensions;

public static class AppVariablesExtensions
{
    public const string TokenKey = "TokenKey";
    public const string RecaptchaSecretKey = "RecaptchaSecretKey";
    public const string AzureCommEmailConnectionStr = "AzureCommEmailConnectionStr";
    public const string SenderEmail = "DoNotReply@e7c11576-5ad4-40aa-a1ad-d6a973c43868.azurecomm.net";

    public const string CollectionUsers = "users";
    public const string CollectionFollows = "follows";
    public const string CollectionMessages = "messages";
    public const string CollectionOnlineTrackers = "online-trackers";
    public const string CollectionExceptionLogs = "exception-logs";

    public static readonly string[] AppVersions = ["1", "1.0.2"];

    public static readonly AppRole[] Roles =
    [
        new() { Name = enums.Roles.Admin.ToString() },
        new() { Name = enums.Roles.Moderator.ToString() },
        new() { Name = enums.Roles.Member.ToString() }
    ];
}