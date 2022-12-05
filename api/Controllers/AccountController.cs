namespace api.Controllers;

[AllowAnonymous] // never use this if you have [Authorize] on the mothods. [Authorize] gets ignored
public class AccountController : BaseApiController {
    private readonly IAccountRepository _accountRepository;
    public AccountController(IAccountRepository accountRepository) {
        _accountRepository = accountRepository;
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginSuccessDto>> Register(UserRegisterDto userIn) {
        LoginSuccessDto? user = await _accountRepository.Create(userIn);
        
        if(user != null && user.BadEmailPattern)
            return BadRequest("Invalid email format.");

        return user == null ? BadRequest("Email is already registered.") : user;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginSuccessDto>> Login(LoginDto userInput) {
        LoginSuccessDto? user = await _accountRepository.Login(userInput);
        return user == null ? BadRequest("Invalid username or password.") : user;
    }
}
