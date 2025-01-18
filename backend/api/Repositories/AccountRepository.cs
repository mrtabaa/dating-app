using System.Text.RegularExpressions;
using IdentityResult = Microsoft.AspNetCore.Identity.IdentityResult;

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

    private async Task<bool> ValidateRecaptcha(string recaptchaToken, CancellationToken cancellationToken) =>
        await _recaptchaService.ValidateTokenAsync(recaptchaToken, cancellationToken);

    private async Task<bool> SendVerificationCode(AppUser appUser, CancellationToken cancellationToken)
    {
        string verificationCode = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);

        return await _emailService.SendVerificationCode(appUser, verificationCode, cancellationToken);
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
        IRecaptchaService recaptchaValidatorService,
        UserManager<AppUser> userManager, ITokenService tokenService,
        IEmailService emailService, IPhotoService photoService
    )
    {
        IMongoDatabase dbName = client.GetDatabase(dbSettings.DatabaseName)
                                ?? throw new ArgumentNullException(nameof(dbName), "The database name cannot be null.");
        _collection = dbName.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);
        _recaptchaService = recaptchaValidatorService;
        _userManager = userManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _photoService = photoService;
    }

    #endregion

    #region CRUD

    public async Task<RegisteredDto> CreateAsync(RegisterDto registerDto, CancellationToken cancellationToken)
    {
        if (!await ValidateRecaptcha(registerDto.RecaptchaToken, cancellationToken))
            return new RegisteredDto(IsRecaptchaTokenInvalid: true);

        AppUser appUser = Mappers.ConvertUserRegisterDtoToAppUser(registerDto);

        IdentityResult userCreatedResult = await _userManager.CreateAsync(appUser, registerDto.Password);
        if (!userCreatedResult.Succeeded)
        {
            return new RegisteredDto(
                ErrorMessage: userCreatedResult.Errors.Select(e => e.Description).FirstOrDefault()
            ); // failed to create the user
        }

        IdentityResult roleResult = await _userManager.AddToRoleAsync(appUser, Roles.Member.ToString());
        if (!roleResult.Succeeded) // Failed to add the role. Delete appUser from DB
        {
            await _userManager.DeleteAsync(appUser);
            return new RegisteredDto();
        }

        if (!await SendVerificationCode(appUser, cancellationToken))
            throw new ArgumentException(nameof(appUser.Email) + ": Failed to email verification code.");

        return new RegisteredDto(
            true
        ); // Account created successfully.
    }

    public async Task<LoggedInDto> VerifyAsync(VerifyDto verifyDto, CancellationToken cancellationToken)
    {
        LoggedInDto loggedInDto = new();

        AppUser? appUser = await _userManager.FindByEmailAsync(verifyDto.Email);
        if (appUser is null)
        {
            loggedInDto.IsEmailNotConfirmed = true;
            return loggedInDto;
        }

        IdentityResult result = await _userManager.ConfirmEmailAsync(appUser, verifyDto.Code);
        if (!result.Succeeded)
        {
            loggedInDto.IsEmailNotConfirmed = true;
            return loggedInDto;
        }

        string? token = await _tokenService.CreateToken(appUser, cancellationToken);

        return !string.IsNullOrEmpty(token)
            ? Mappers.ConvertAppUserToLoggedInDto(appUser, token, GetMainPhoto(appUser))
            : loggedInDto;
    }

    public async Task<ResendCodeResult> ResendVerifyCodeAsync(ResendCodeRequest resendCRequest, CancellationToken cancellationToken)
    {
        if (!await ValidateRecaptcha(resendCRequest.RecaptchaToken, cancellationToken))
            return new ResendCodeResult(true);

        AppUser? appUser = await _userManager.FindByEmailAsync(resendCRequest.Email);

        return appUser is null
            ? new ResendCodeResult()
            : new ResendCodeResult(IsSuccessful: await SendVerificationCode(appUser, cancellationToken));
    }

    public async Task<LoggedInDto> LoginAsync(LoginDto userInput, CancellationToken cancellationToken)
    {
        LoggedInDto loggedInDto = new();

        if (!await ValidateRecaptcha(userInput.RecaptchaToken, cancellationToken))
        {
            loggedInDto.IsRecaptchaTokenInvalid = true;
            return loggedInDto;
        }

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

        if (!await _userManager.IsEmailConfirmedAsync(appUser))
        {
            if (!await SendVerificationCode(appUser, cancellationToken))
                throw new ArgumentException(nameof(appUser.UserName) + ": Failed to email verification code.");

            loggedInDto.Email = appUser.Email;
            loggedInDto.IsEmailNotConfirmed = true;
            return loggedInDto;
        }

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