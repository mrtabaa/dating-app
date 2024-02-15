namespace api.Interfaces;

public interface IAccountRepository
{
    public Task<LoggedInDto> CreateAsync(UserRegisterDto registerDto, CancellationToken cancellationToken);
    public Task<LoggedInDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken);
    public Task<LoggedInDto?> GetLoggedInUserAsync(string? userEmail, string? token, CancellationToken cancellationToken);
    public Task<DeleteResult?> DeleteUserAsync(string? userEmail, CancellationToken cancellationToken);
}