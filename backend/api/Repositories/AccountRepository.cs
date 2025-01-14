using System.Text.RegularExpressions;
using api.DTOs.helpers;

namespace api.Repositories;

public class AccountRepository : IAccountRepository
{
    /// <summary>
    ///     This function gets appUser's main photo's Url_165 and convert it to blobUriWithSas
    /// </summary>
    /// <param name="appUser"></param>
    /// <returns>string blobUriWithSas</returns>
    private string? GetMainPhoto(AppUser appUser) =>
        _photoService.ConvertPhotoToBlobLinkWithSas(appUser.Photos.FirstOrDefault(photo => photo.IsMain))?.Url165;

    private async Task<LoggedInDto> ValidateRecaptcha(string recaptchaToken, LoggedInDto loggedInDto, CancellationToken cancellationToken)
    {
        bool isValid = await _recaptchaService.ValidateTokenAsync(recaptchaToken, cancellationToken);

        loggedInDto.IsRecaptchaTokenInvalid = !isValid;

        return loggedInDto;
    }

    private async Task<bool> VerifyAccount(string recipientEmail, CancellationToken cancellationToken)
    {
        const string verificationLink = "https://hallboard.com/account/verify";

        var request = new EmailRequest(
            "mrtabaa@gmail.com",
            "Account Verification",
            $"""
             		<html>
             			<body>
             				<h1>Verify your email using this link {verificationLink}.</h1>
             			</body>
             		</html>
             """
        );

        return await _emailService.SendEmailAsync(request, cancellationToken);
    }

    #region Db and Token Settings

    private readonly IMongoCollection<AppUser> _collection;
    private readonly IRecaptchaService _recaptchaService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService; // save user credential as a token
    private readonly IEmailService _emailService;
    private readonly IPhotoService _photoService;

    // constructor - dependency injection
    public AccountRepository(
        IMongoClient client, IMyMongoDbSettings dbSettings,
        IRecaptchaService turnstileValidatorService,
        UserManager<AppUser> userManager, ITokenService tokenService,
        IEmailService emailService, IPhotoService photoService
    )
    {
        IMongoDatabase dbName = client.GetDatabase(dbSettings.DatabaseName)
                                ?? throw new ArgumentNullException(nameof(dbName), "The database name cannot be null.");
        _collection = dbName.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);
        _recaptchaService = turnstileValidatorService;
        _userManager = userManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _photoService = photoService;
    }

    #endregion

    #region CRUD

    public async Task<LoggedInDto> CreateAsync(RegisterDto registerDto, CancellationToken cancellationToken)
    {
        LoggedInDto loggedInDto = new();

        loggedInDto = await ValidateRecaptcha(registerDto.RecaptchaToken, loggedInDto, cancellationToken);
        if (loggedInDto.IsRecaptchaTokenInvalid)
            return loggedInDto;

        #region Create user, token and add role

        AppUser appUser = Mappers.ConvertUserRegisterDtoToAppUser(registerDto);

        IdentityResult userCreatedResult = await _userManager.CreateAsync(appUser, registerDto.Password);

        if (userCreatedResult.Succeeded)
        {
            IdentityResult roleResult = await _userManager.AddToRoleAsync(appUser, Roles.Member.ToString());

            if (!roleResult.Succeeded) // failed
                return loggedInDto;

            string? token = await _tokenService.CreateToken(appUser, cancellationToken);

            if (!string.IsNullOrEmpty(token))
                return Mappers.ConvertAppUserToLoggedInDto(appUser, token, GetMainPhoto(appUser)); // Return loggedInDto
        }
        else // Store and return userCreatedResult errors if failed. 
        {
            foreach (IdentityError error in userCreatedResult.Errors)
                loggedInDto.Errors.Add(error.Description);
        }

        #endregion Create user, token and role

        return loggedInDto; // failed
    }

    public async Task<LoggedInDto> LoginAsync(LoginDto userInput, CancellationToken cancellationToken)
    {
        LoggedInDto loggedInDto = new();

        // loggedInDto = await ValidateRecaptcha(userInput.RecaptchaToken, loggedInDto, cancellationToken);
        // if (loggedInDto.IsRecaptchaTokenInvalid)
        //     return loggedInDto;
        
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
        
        bool isEmailSent = await VerifyAccount(userInput.EmailUsername, cancellationToken);

        string? token = await _tokenService.CreateToken(appUser, cancellationToken);

        return !string.IsNullOrEmpty(token)
            ? Mappers.ConvertAppUserToLoggedInDto(appUser, token, GetMainPhoto(appUser), userInput.RecaptchaToken)
            : loggedInDto;
    }

    public async Task<LoggedInDto?> ReloadLoggedInUserAsync(string userIdHashed, string token, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(userIdHashed, cancellationToken);

        if (userId is null) return null;

        AppUser appUser = await _collection.Find(appUser => appUser.Id == userId).FirstOrDefaultAsync(cancellationToken);

        return appUser is null
            ? null
            : Mappers.ConvertAppUserToLoggedInDto(appUser, token, GetMainPhoto(appUser));
    }

    public async Task<UpdateResult?> UpdateLastActive(string userIdHashed, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(userIdHashed, cancellationToken);

        if (userId is null) return null;

        UpdateDefinition<AppUser> updatedUserLastActive = Builders<AppUser>.Update
            .Set(appUser => appUser.LastActive, DateTime.UtcNow);

        return await _collection.UpdateOneAsync(appUser => appUser.Id == userId, updatedUserLastActive, null, cancellationToken);
    }

    public async Task<DeleteResult?> DeleteUserAsync(string? userEmail, CancellationToken cancellationToken) =>
        await _collection.DeleteOneAsync(appUser => appUser.Email == userEmail, cancellationToken);

    #endregion CRUD
}