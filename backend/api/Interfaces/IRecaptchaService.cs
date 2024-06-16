namespace api.Interfaces;

public interface IRecaptchaService
{
    public Task<bool> ValidateTokenAsync(string? turnstileToken, CancellationToken cancellationToken);
}
