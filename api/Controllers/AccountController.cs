namespace api.Controllers;

public class AccountController : BaseApiController {
    private readonly IAccountRepository _accountRepository;
    public AccountController(IAccountRepository accountRepository) {
        _accountRepository = accountRepository;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<LoginSuccessDto>> Register(UserRegisterDto userIn) {
        LoginSuccessDto? user = await _accountRepository.Create(userIn);
        return user == null ? BadRequest("Email is already registered.") : user;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginSuccessDto>> Login(LoginDto userInput) {
        LoginSuccessDto? user = await _accountRepository.Login(userInput);
        return user == null ? BadRequest("Invalid username or password.") : user;
    }
}
