namespace api.Extensions;

internal static class EmailExtensions
{
    internal const string AzureCommEmailConnectionStr = "AzureCommEmailConnectionStr";
    internal const string SenderEmail = "DoNotReply@hallboard.com";
    internal const string VerifySubject = "Verify Your Hallboard Account";
    internal const string RecoverySubject = "Recover Your Hallboard Account";

    //TODO: Improve layout
    internal static string GetVerificationTemplate(string verificationCode) =>
        $"""
                <html>
                    <body style="display: flex; flex-direction= column; align-items=center">
                        <h5>Verify your account using this verification code</h5>
                        <h2>{verificationCode}</h2>
                    </body>
                </html>
         """;

    //TODO: Improve layout
    internal static string GetRecoveryTemplate(string verificationCode) =>
        $"""
                <html>
                    <body style="display: flex; flex-direction= column; align-items=center">
                        <h5>Reset your password using this verification code</h5>
                        <h2>{verificationCode}</h2>
                    </body>
                </html>
         """;
}