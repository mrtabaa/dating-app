namespace api.Interfaces;

public interface IEmailService
{
    public Task<bool> SendVerificationCode(AppUser appUser, string verificationCode, CancellationToken cancellationToken);
    public Task<bool> SendRecoveryCode(AppUser appUser, string verificationCode, CancellationToken cancellationToken);
}