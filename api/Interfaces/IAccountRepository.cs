namespace api.Interfaces;

public interface IAccountRepository
{
    public Task<UserDto?> CreateAsync(UserRegisterDto userIn, CancellationToken cancellationToken);
    public Task<UserDto?> LoginAsync(LoginDto userIn, CancellationToken cancellationToken);
}