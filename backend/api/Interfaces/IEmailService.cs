namespace api.Interfaces;

public interface IEmailService
{
    public Task<bool> SendVerificationCode(
        AppUser appUser, string verificationCode, CancellationToken cancellationToken
    );

    public Task<bool> SendPasswordResetLink(AppUser appUser, string resetLink, CancellationToken cancellationToken);
    public Task<bool> SendResetPasswordConfirmation(AppUser appUser);
}