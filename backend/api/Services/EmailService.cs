using Azure;
using Azure.Communication.Email;

namespace api.Services;

public class EmailService(IConfiguration config) : IEmailService
{
    private readonly string? _connectionString = config.GetValue<string>(EmailExtensions.AzureCommEmailConnectionStr)
                                                 ?? throw new NullReferenceException(nameof(_connectionString));

    public async Task<bool> SendVerificationCode(AppUser appUser, string verificationCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(appUser.Email))
            throw new ArgumentNullException(nameof(appUser.Email));

        var request = new EmailRequest(
            appUser.Email.ToLower(),
            EmailExtensions.VerifySubject,
            EmailExtensions.GetVerificationTemplate(verificationCode, appUser.UserName
                                                                      ?? throw new ArgumentNullException(
                                                                          nameof(appUser.UserName), "cannot be null."))
        );

        return await SendEmailAsync(request, cancellationToken);
    }

    public async Task<bool> SendPasswordResetLink(AppUser appUser, string resetLink, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(appUser.Email))
            throw new ArgumentNullException(nameof(appUser.Email));

        var request = new EmailRequest(
            appUser.Email.ToLower(),
            EmailExtensions.RecoverySubject,
            EmailExtensions.GetResetPasswordTemplate(resetLink, appUser.UserName
                                                                ?? throw new ArgumentNullException(
                                                                    nameof(appUser.UserName), "cannot be null."))
        );

        return await SendEmailAsync(request, cancellationToken);
    }

    private async Task<bool> SendEmailAsync(EmailRequest request, CancellationToken cancellationToken)
    {
        // Create an EmailClient using the connection string
        var emailClient = new EmailClient(_connectionString);

        var emailMessage = new EmailMessage(
            EmailExtensions.SenderEmail,
            content: new EmailContent(request.Subject)
            {
                Html = request.Body
            },
            recipients: new EmailRecipients(new List<EmailAddress> { new(request.ToEmail) })
        );

        EmailSendOperation emailSendOperation = await emailClient.SendAsync(WaitUntil.Completed, emailMessage, cancellationToken);

        EmailSendStatus result = emailSendOperation.Value.Status;

        return emailSendOperation.Value.Status == "Succeeded";
    }
}