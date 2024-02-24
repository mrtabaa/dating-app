namespace api.Interfaces;

public interface IAccountRepository
{
    public Task<LoggedInDto> CreateAsync(RegisterDto registerDto, CancellationToken cancellationToken);
    public Task<LoggedInDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken);
    public Task<LoggedInDto?> GetLoggedInUserAsync(string? userEmail, string? token, CancellationToken cancellationToken);
    public Task<DeleteResult?> DeleteUserAsync(string? userEmail, CancellationToken cancellationToken);
    public Task<UpdateResult?> UpdateLastActive(string loggedInUserIdHashed, CancellationToken cancellationToken);
}