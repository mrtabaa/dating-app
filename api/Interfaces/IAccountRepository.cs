namespace api.Interfaces;

public interface IAccountRepository
{
    public Task<UserDto?> Create(UserRegisterDto userIn);
    public Task<UserDto?> Login(LoginDto userIn);
}