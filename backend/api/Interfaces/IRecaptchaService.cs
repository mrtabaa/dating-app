namespace api.Interfaces;

public interface IRecaptchaService
{
    public Task<bool> ValidateTokenAsync(string? recaptchaToken, CancellationToken cancellationToken);
}