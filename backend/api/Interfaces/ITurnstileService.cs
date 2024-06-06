namespace api.Interfaces;

public interface ITurnstileService
{
    public Task<bool> ValidateTokenAsync(string? turnstileToken, CancellationToken cancellationToken);
}
