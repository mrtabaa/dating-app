using api.DTOs.helpers;

namespace api.Interfaces;

public interface IEmailService
{
    public Task<bool> SendEmailAsync(EmailRequest request, CancellationToken cancellationToken);
}