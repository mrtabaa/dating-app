namespace api.Interfaces;
public interface IAccountRepository
{
    public Task<LoginSuccessDto?> Create(UserRegisterDto userIn);
    public Task<LoginSuccessDto?> Login(LoginDto userIn);
}