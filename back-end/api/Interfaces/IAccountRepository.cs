namespace api.Interfaces;

public interface IAccountRepository
{
    public Task<LoggedInDto?> CreateAsync(UserRegisterDto userIn, CancellationToken cancellationToken);
    public Task<LoggedInDto?> LoginAsync(LoginDto userIn, CancellationToken cancellationToken);
    public Task<LoggedInDto?> GetLoggedInUserAsync(string? userId, string? token, CancellationToken cancellationToken);
}