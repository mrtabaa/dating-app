namespace api.Extensions;

internal static class EmailExtensions
{
    internal const string AzureCommEmailConnectionStr = "AzureCommEmailConnectionStr";
    internal const string SenderEmail = "DoNotReply@hallboard.com";
    internal const string VerifySubject = "Verify Your Hallboard Account";
    internal const string RecoverySubject = "Recover Your Hallboard Account";
    internal const string ResetPassConfirmationSubject = "Reset password Confirmation | Hallboard Account";

    //TODO: Improve layout
    internal static string GetVerificationTemplate(string verificationCode, string userName) =>
        $"""
                <html>
                    <body style="display: flex; flex-direction= column; align-items=center">
                        <p>Dear {userName},</p>
                        <p>Please verify your account using this verification code:</p>
                        <h3>{verificationCode}</h3>
                    </body>
                </html>
         """;

    //TODO: Improve layout
    internal static string GetResetPasswordTemplate(string resetLink, string userName) =>
        $"""
                <html>
                    <body style="display: flex; flex-direction= column; align-items=center">
                        <p>Dear {userName},</p>
                        <p>You may reset your password from the page below:</p>
                        <a href="{resetLink}" style="width: fit-content;">Reset password page</a>
                    </body>
                </html>
         """;

    internal static string GetResetPasswordConfirmationTemplate(string userName) =>
        $"""
                <html>
                    <body style="display: flex; flex-direction= column; align-items=center">
                        <p>Dear {userName},</p>
                        <p>Your password was reset successfully on {DateTime.UtcNow}. You can now login with your new password.</p>
                    </body>
                </html>
         """;
}