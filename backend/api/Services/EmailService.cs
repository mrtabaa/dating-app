using api.DTOs.helpers;
using Azure;
using Azure.Communication.Email;

namespace api.Services;

public class EmailService(IConfiguration config) : IEmailService
{
    private readonly string? _connectionString = config.GetValue<string>(AppVariablesExtensions.AzureCommEmailConnectionStr)
                                                 ?? throw new NullReferenceException(nameof(_connectionString));

    public async Task<bool> SendEmailAsync(EmailRequest request, CancellationToken cancellationToken)
    {
        // Create an EmailClient using the connection string
        var emailClient = new EmailClient(_connectionString);

        var emailMessage = new EmailMessage(
            AppVariablesExtensions.SenderEmail,
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