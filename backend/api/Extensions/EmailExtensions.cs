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
    internal static string GetRecoveryTemplate(string verificationCode, string userName) =>
        $"""
                <html>
                    <body style="display: flex; flex-direction= column; align-items=center">
                        <h5>Dear {userName},</h5>
                        <h5>You can reset your password using this verification code:</h5>
                        <h2>{verificationCode}</h2>
                    </body>
                </html>
         """;
}