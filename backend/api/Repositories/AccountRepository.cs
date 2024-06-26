using System.Text.RegularExpressions;

namespace api.Repositories;

public class AccountRepository : IAccountRepository
{
    #region Db and Token Settings
    private readonly IMongoCollection<AppUser>? _collection;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService; // save user credential as a token

    // constructor - dependency injection
    public AccountRepository(IMongoClient client, IMyMongoDbSettings dbSettings, UserManager<AppUser> userManager, ITokenService tokenService)
    {
        var database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AppUser>(AppVariablesExtensions.collectionUsers);

        _userManager = userManager;
        _tokenService = tokenService;
    }
    #endregion

    #region CRUD
    public async Task<LoggedInDto> CreateAsync(RegisterDto registerDto, CancellationToken cancellationToken)
    {
        LoggedInDto loggedInDto = new();

        #region Create user, token and add role

        AppUser appUser = Mappers.ConvertUserRegisterDtoToAppUser(registerDto);

        IdentityResult? userCreatedResult = await _userManager.CreateAsync(appUser, registerDto.Password);

        if (userCreatedResult.Succeeded)
        {
            IdentityResult? roleResult = await _userManager.AddToRoleAsync(appUser, Roles.member.ToString());

            if (!roleResult.Succeeded) // failed
                return loggedInDto;

            string? token = await _tokenService.CreateToken(appUser, cancellationToken);

            if (!string.IsNullOrEmpty(token))
            {
                return Mappers.ConvertAppUserToLoggedInDto(appUser, token); // returns LoggedInDto
            }
        }
        else // Store and return userCreatedResult errors if failed. 
        {
            foreach (IdentityError error in userCreatedResult.Errors)
            {
                loggedInDto.Errors.Add(error.Description);
            }
        }
        #endregion Create user, token and role

        return loggedInDto; // failed
    }

    public async Task<LoggedInDto> LoginAsync(LoginDto userInput, CancellationToken cancellationToken)
    {
        LoggedInDto loggedInDto = new();

        AppUser? appUser;

        // Find appUser by Email or UserName
        if (Regex.IsMatch(userInput.EmailUsername, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$")) // If input is email
            appUser = await _userManager.FindByEmailAsync(userInput.EmailUsername);
        else
            appUser = await _userManager.FindByNameAsync(userInput.EmailUsername);

        if (appUser is null)
        {
            loggedInDto.IsWrongCreds = true;
            return loggedInDto;
        }

        if (!await _userManager.CheckPasswordAsync(appUser, userInput.Password))
        {
            loggedInDto.IsWrongCreds = true;
            return loggedInDto;
        }

        string? token = await _tokenService.CreateToken(appUser, cancellationToken);

        if (!string.IsNullOrEmpty(token))
        {
            return Mappers.ConvertAppUserToLoggedInDto(appUser, token);
        }

        return loggedInDto;
    }

    public async Task<LoggedInDto?> ReloadLoggedInUser(string userIdHashed, string token, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserId(userIdHashed, cancellationToken);

        if (!userId.HasValue || userId.Value.Equals(ObjectId.Empty)) return null;

        AppUser appUser = await _collection.Find<AppUser>(appUser => appUser.Id == userId).FirstOrDefaultAsync(cancellationToken);

        return appUser is null
            ? null
            : Mappers.ConvertAppUserToLoggedInDto(appUser, token);
    }

    public async Task<UpdateResult?> UpdateLastActive(string userIdHashed, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserId(userIdHashed, cancellationToken);

        if (!userId.HasValue || userId.Value.Equals(ObjectId.Empty)) return null;

        UpdateDefinition<AppUser> updatedUserLastActive = Builders<AppUser>.Update
            .Set(appUser => appUser.LastActive, DateTime.UtcNow);

        return await _collection.UpdateOneAsync<AppUser>(appUser => appUser.Id == userId, updatedUserLastActive, null, cancellationToken);
    }

    public async Task<DeleteResult?> DeleteUserAsync(string? userEmail, CancellationToken cancellationToken) =>
       await _collection.DeleteOneAsync<AppUser>(appUser => appUser.Email == userEmail, cancellationToken);
    #endregion CRUD
}
