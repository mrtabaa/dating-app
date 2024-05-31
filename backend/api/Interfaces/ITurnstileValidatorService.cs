namespace api.Interfaces;

public interface ITurnstileValidatorService
{
    public Task<bool> ValidateTokenAsync(string turnstileToken, CancellationToken cancellationToken);
}
