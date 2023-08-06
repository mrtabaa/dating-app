namespace api.Interfaces;

public interface IAccountRepository
{
    public Task<UserDto?> Create(UserRegisterDto userIn, CancellationToken cancellationToken);
    public Task<UserDto?> Login(LoginDto userIn, CancellationToken cancellationToken);
}