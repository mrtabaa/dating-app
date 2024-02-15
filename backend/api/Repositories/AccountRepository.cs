using Microsoft.AspNetCore.Identity;


namespace api.Repositories;

public class AccountRepository : IAccountRepository
{

    #region Db and Token Settings
    private readonly IMongoCollection<AppUser>? _collection;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly ITokenService _tokenService; // save user credential as a token

    // constructor - dependency injection
    public AccountRepository(IMongoClient client, IMyMongoDbSettings dbSettings, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ITokenService tokenService)
    {
        var database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AppUser>(AppVariablesExtensions.collectionUsers);
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
    }
    #endregion

    #region CRUD
    public async Task<LoggedInDto> CreateAsync(UserRegisterDto registerDto, CancellationToken cancellationToken)
    {
        // _userManager.Users doesn't have AnyAsync so use MongoDriver here
        bool userExist = await _collection.Find<AppUser>(appUser => appUser.Email == registerDto.Email.ToLower().Trim()).AnyAsync(cancellationToken);

        if (userExist) return Mappers.ConvertAppUserToLoggedInDto(isAlreadyExist: true);

        AppUser appUser = Mappers.ConvertUserRegisterDtoToAppUser(registerDto);

        var userCreated = _userManager.CreateAsync(appUser, registerDto.Password);

        string? token = await _tokenService.CreateToken(appUser, cancellationToken);

        if (!userCreated.Result.Succeeded || string.IsNullOrEmpty(token))
            return Mappers.ConvertAppUserToLoggedInDto(isFailed: true);

        return Mappers.ConvertAppUserToLoggedInDto(appUser, token); // success
    }

    public async Task<LoggedInDto> LoginAsync(LoginDto userInput, CancellationToken cancellationToken)
    {
        AppUser? appUser = await _userManager.FindByEmailAsync(userInput.Email);

        if (appUser is null || !await _userManager.CheckPasswordAsync(appUser, userInput.Password)) 
            return Mappers.ConvertAppUserToLoggedInDto(isWrongCreds: true);

        string? token = await _tokenService.CreateToken(appUser, cancellationToken);

        if (string.IsNullOrEmpty(token))
            return Mappers.ConvertAppUserToLoggedInDto(isFailed: true);

        return Mappers.ConvertAppUserToLoggedInDto(appUser, token);
    }

    public async Task<LoggedInDto?> GetLoggedInUserAsync(string? userEmail, string? token, CancellationToken cancellationToken)
    {
        if (!(userEmail is null || token is null))
        {
            AppUser appUser = await _collection.Find<AppUser>(appUser => appUser.Email == userEmail).FirstOrDefaultAsync(cancellationToken);

            return appUser is null ? null : Mappers.ConvertAppUserToLoggedInDto(appUser, token, true);
        }

        return null;
    }

    public async Task<DeleteResult?> DeleteUserAsync(string? userEmail, CancellationToken cancellationToken) =>
       await _collection.DeleteOneAsync<AppUser>(appUser => appUser.Email == userEmail, cancellationToken);
    #endregion CRUD

    // private async Task<string> GetToken(string token)
}
