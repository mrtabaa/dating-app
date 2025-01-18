namespace api.Extensions;

internal static class EmailExtensions
{
    internal const string AzureCommEmailConnectionStr = "AzureCommEmailConnectionStr";
    internal const string SenderEmail = "DoNotReply@hallboard.com";
    internal const string VerifySubject = "Verify Your Hallboard Account";
    internal const string RecoverySubject = "Recover Your Hallboard Account";

    //TODO: Improve layout
    internal static string GetVerificationTemplate(string verificationCode, string userName) =>
        $"""
                <html>
                    <body style="display: flex; flex-direction= column; align-items=center">
                        <h5>Dear {userName},</h5>
                        <h5>Please verify your account using this verification code:</h5>
                        <h2>{verificationCode}</h2>
                    </body>
                </html>
         """;

    //TODO: Improve layout
    internal static string GetResetPasswordTemplate(string resetLink, string userName) =>
        $"""
                <html>
                    <body style="display: flex; flex-direction= column; align-items=center">
                        <h5>Dear {userName},</h5>
                        <h5>You may reset your password from the page below:</h5>
                        <a href="{resetLink}" style="width: fit-content;">Reset password page</a>
                    </body>
                </html>
         """;
}