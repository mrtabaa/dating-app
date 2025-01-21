using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;
using api.DTOs.Account;
using api.DTOs.Helpers;
using IdentityResult = Microsoft.AspNetCore.Identity.IdentityResult;

namespace api.Repositories;

public class AccountRepository : IAccountRepository
{
    #region ErrorMessages

    private const string RecaptchaErrorMessage = "Recaptcha token is invalid. 'Slide me!' again.";
    private const string WrongCredsErrorMessage = "Wrong username or password.";
    private const string EmailNotConfrimedErrorMessage = "Please confirm your email using the code sent to your email.";

    private const string EmailAlreadyConfirmedErrorMessage = "This email is already registered and verified. " +
                                                             "You can login or recover your password if the owner.";

    #endregion

    #region Helpers

    private async Task<OperationResult<bool>> RegisterIfEmailAlreadyExists(
        AppUser existingUser, RegisterDto registerDto, CancellationToken cancellationToken)
    {
        if (await _userManager.IsEmailConfirmedAsync(existingUser))
        {
            return new OperationResult<bool>(
                Error: new CustomError(
                    ErrorCode.IsEmailAlreadyConfirmed,
                    EmailAlreadyConfirmedErrorMessage
                )
            );
        }

        // Update the unverified user's details for the real owner
        existingUser.Gender = registerDto.Gender;
        existingUser.UserName = registerDto.UserName;
        existingUser.DateOfBirth = registerDto.DateOfBirth;
        existingUser.PasswordHash = _userManager.PasswordHasher.HashPassword(existingUser, registerDto.Password);
        existingUser.Roles = [];

        IdentityResult updateResult = await _userManager.UpdateAsync(existingUser);
        if (!updateResult.Succeeded)
        {
            return new OperationResult<bool>(
                Error: new CustomError(
                    ErrorCode.NetIdentity,
                    updateResult.Errors.FirstOrDefault()?.Description
                )
            );
        }

        IdentityResult roleResult = await _userManager.AddToRoleAsync(existingUser, Roles.Member.ToString());
        if (!roleResult.Succeeded) // Failed to add the role. Delete appUser from DB
        {
            await _userManager.DeleteAsync(existingUser);
            return new OperationResult<bool>();
        }

        // Resend the verification email
        if (!await SendVerificationCode(existingUser, cancellationToken))
            throw new ArgumentException(nameof(existingUser.Email) + ": Failed to email verification code.");

        return new OperationResult<bool>(true); // Account created successfully.
    }

    /// <summary>
    ///     This function gets appUser's main photo's Url_165 and convert it to blobUriWithSas
    /// </summary>
    /// <param name="appUser"></param>
    /// <returns>string blobUriWithSas</returns>
    private string? GetMainPhoto(AppUser appUser) =>
        _photoService.ConvertPhotoToBlobLinkWithSas(appUser.Photos.FirstOrDefault(photo => photo.IsMain))?.Url165;

    private async Task<bool> ValidateRecaptcha(string recaptchaToken, CancellationToken cancellationToken) =>
        _hostEnvironment.IsDevelopment() || // Validate in Development
        await _recaptchaService.ValidateTokenAsync(recaptchaToken, cancellationToken);

    private async Task<bool> SendVerificationCode(AppUser appUser, CancellationToken cancellationToken)
    {
        string verificationCode = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);

        return await _emailService.SendVerificationCode(appUser, verificationCode, cancellationToken);
    }

    private static string GenerateResetPasswordLink(string baseUrl, string email, string resetToken)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) throw new ArgumentException("Base URL cannot be null or empty.", nameof(baseUrl));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be null or empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(resetToken)) throw new ArgumentException("Token cannot be null or empty.", nameof(resetToken));

        var builder = new UriBuilder(baseUrl);
        NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);

        query.Add("email", email);
        query.Add("resetToken", resetToken);
        builder.Query = query.ToString();

        return builder.Uri.ToString();
    }

    #endregion

    #region Db and Token Settings

    private readonly IMongoCollection<AppUser> _collection;
    private readonly IRecaptchaService _recaptchaService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService; // save user credential as a token
    private readonly IEmailService _emailService;
    private readonly IPhotoService _photoService;
    private readonly IHostEnvironment _hostEnvironment;

    // constructor - dependency injection
    public AccountRepository(
        IMongoClient client, IMyMongoDbSettings dbSettings,
        IRecaptchaService recaptchaValidatorService,
        UserManager<AppUser> userManager, ITokenService tokenService,
        IEmailService emailService, IPhotoService photoService, IHostEnvironment hostEnvironment
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
        _hostEnvironment = hostEnvironment;
    }

    #endregion

    #region CRUD

    public async Task<OperationResult<bool>> CreateAsync(RegisterDto registerDto, CancellationToken cancellationToken)
    {
        if (!await ValidateRecaptcha(registerDto.RecaptchaToken, cancellationToken))
        {
            return new OperationResult<bool>(
                Error: new CustomError(
                    ErrorCode.IsRecaptchaTokenInvalid,
                    RecaptchaErrorMessage
                )
            );
        }

        AppUser? existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
            return await RegisterIfEmailAlreadyExists(existingUser, registerDto, cancellationToken);

        // Proceed with creating a new user if no existing user was found
        AppUser appUser = Mappers.ConvertRegisterDtoToAppUser(registerDto);

        IdentityResult userCreatedResult = await _userManager.CreateAsync(appUser, registerDto.Password);
        if (!userCreatedResult.Succeeded)
        {
            // failed to create the user
            return new OperationResult<bool>(
                Error: new CustomError(
                    ErrorCode.NetIdentity,
                    userCreatedResult.Errors.Select(e => e.Description).FirstOrDefault()
                )
            );
        }

        IdentityResult roleResult = await _userManager.AddToRoleAsync(appUser, Roles.Member.ToString());
        if (!roleResult.Succeeded) // Failed to add the role. Delete appUser from DB
        {
            await _userManager.DeleteAsync(appUser);
            return new OperationResult<bool>();
        }

        if (!await SendVerificationCode(appUser, cancellationToken))
            throw new ArgumentException(nameof(appUser.Email) + ": Failed to email verification code.");

        return new OperationResult<bool>(true); // Account created successfully.
    }

    public async Task<OperationResult<LoggedInDto>> VerifyAsync(VerifyDto verifyDto, CancellationToken cancellationToken)
    {
        AppUser? appUser = await _userManager.FindByEmailAsync(verifyDto.Email);
        if (appUser is null)
            return new OperationResult<LoggedInDto>();

        if (await _userManager.IsEmailConfirmedAsync(appUser))
        {
            return new OperationResult<LoggedInDto>(
                Error: new CustomError(
                    ErrorCode.IsEmailAlreadyConfirmed,
                    EmailAlreadyConfirmedErrorMessage
                )
            );
        }

        IdentityResult result = await _userManager.ConfirmEmailAsync(appUser, verifyDto.Code);
        if (!result.Succeeded)
            return new OperationResult<LoggedInDto>();

        string? token = await _tokenService.CreateToken(appUser, cancellationToken);

        return string.IsNullOrEmpty(token)
            ? new OperationResult<LoggedInDto>()
            : new OperationResult<LoggedInDto>(
                true,
                Mappers.ConvertAppUserToLoggedInDto(appUser, token, GetMainPhoto(appUser)
                )
            );
    }

    public async Task<OperationResult<bool>> ResendVerifyCodeAsync(ResendCodeRequest resendCRequest, CancellationToken cancellationToken)
    {
        if (!await ValidateRecaptcha(resendCRequest.RecaptchaToken, cancellationToken))
        {
            return new OperationResult<bool>(
                Error: new CustomError(
                    ErrorCode.IsRecaptchaTokenInvalid,
                    RecaptchaErrorMessage
                )
            );
        }

        AppUser? appUser = await _userManager.FindByEmailAsync(resendCRequest.Email);
        if (appUser is null) return new OperationResult<bool>();

        if (await _userManager.IsEmailConfirmedAsync(appUser))
        {
            return new OperationResult<bool>(
                Error: new CustomError(
                    ErrorCode.IsEmailAlreadyConfirmed,
                    EmailAlreadyConfirmedErrorMessage
                )
            );
        }

        return new OperationResult<bool>(await SendVerificationCode(appUser, cancellationToken)); // Success depends on the email sent success
    }

    public async Task<OperationResult<LoggedInDto>> LoginAsync(LoginDto userInput, CancellationToken cancellationToken)
    {
        if (!await ValidateRecaptcha(userInput.RecaptchaToken, cancellationToken))
        {
            return new OperationResult<LoggedInDto>(
                Error: new CustomError(
                    ErrorCode.IsRecaptchaTokenInvalid,
                    RecaptchaErrorMessage
                )
            );
        }

        AppUser? appUser;

        // Find appUser by Email or UserName
        if (Regex.IsMatch(userInput.EmailUsername, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$")) // If input is email
            appUser = await _userManager.FindByEmailAsync(userInput.EmailUsername);
        else
            appUser = await _userManager.FindByNameAsync(userInput.EmailUsername);

        if (appUser is null)
        {
            return new OperationResult<LoggedInDto>(
                Error: new CustomError(
                    ErrorCode.IsWrongCreds,
                    WrongCredsErrorMessage
                )
            );
        }

        if (!await _userManager.CheckPasswordAsync(appUser, userInput.Password))
        {
            return new OperationResult<LoggedInDto>(
                Error: new CustomError(
                    ErrorCode.IsWrongCreds,
                    WrongCredsErrorMessage
                )
            );
        }

        if (!await _userManager.IsEmailConfirmedAsync(appUser))
        {
            if (!await SendVerificationCode(appUser, cancellationToken))
                throw new ArgumentException(nameof(appUser.UserName) + ": Failed to email verification code.");

            return new OperationResult<LoggedInDto>(
                Error: new CustomError(
                    ErrorCode.IsEmailNotConfirmed,
                    EmailNotConfrimedErrorMessage
                )
            );
        }

        string? token = await _tokenService.CreateToken(appUser, cancellationToken);

        return string.IsNullOrEmpty(token)
            ? new OperationResult<LoggedInDto>()
            : new OperationResult<LoggedInDto>(
                true,
                Mappers.ConvertAppUserToLoggedInDto(appUser, token, GetMainPhoto(appUser)
                )
            );
    }

    public async Task<OperationResult<LoggedInDto>> ReloadLoggedInUserAsync(string userIdHashed, string token, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(userIdHashed, cancellationToken);

        if (userId is null)
            return new OperationResult<LoggedInDto>();

        AppUser appUser = await _collection.Find(appUser => appUser.Id == userId).FirstOrDefaultAsync(cancellationToken);

        return appUser is null
            ? new OperationResult<LoggedInDto>()
            : new OperationResult<LoggedInDto>(
                true,
                Mappers.ConvertAppUserToLoggedInDto(appUser, token, GetMainPhoto(appUser))
            );
    }

    public async Task<OperationResult<bool>> RequestResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        if (!await ValidateRecaptcha(request.RecaptchaToken, cancellationToken))
        {
            return new OperationResult<bool>(
                Error: new CustomError(
                    ErrorCode.IsRecaptchaTokenInvalid,
                    RecaptchaErrorMessage
                )
            );
        }

        AppUser? appUser = await _userManager.FindByEmailAsync(request.Email.Trim());

        if (appUser is null || string.IsNullOrEmpty(appUser.Email))
            return new OperationResult<bool>();

        string resetToken = await _userManager.GeneratePasswordResetTokenAsync(appUser);

        string resetLink = GenerateResetPasswordLink(
            // "http://localhost:4300/account/reset-password",
            "https://www.hallboard.com/account/reset-password",
            appUser.Email, resetToken);

        if (!await _emailService.SendPasswordResetLink(appUser, resetLink, cancellationToken))
            throw new ArgumentException("Failed to send reset password link. Check if email provider is working.", nameof(appUser.Email));

        return new OperationResult<bool>();
    }

    public async Task<OperationResult<bool>> ResetPasswordAsync(ResetPassword resetPassword, CancellationToken cancellationToken)
    {
        AppUser? appUser = await _userManager.FindByEmailAsync(resetPassword.Email.Trim());
        if (appUser is null)
            return new OperationResult<bool>();

        IdentityResult passwordResetResult = await _userManager
            .ResetPasswordAsync(appUser, resetPassword.ResetToken, resetPassword.Password);

        if (passwordResetResult.Succeeded)
        {
            // TODO: Email the password change warning/confirmation.
        }

        return new OperationResult<bool>(passwordResetResult.Succeeded);
    }

    public async Task<OperationResult<DeleteResult>> DeleteUserAsync(string? userEmail, CancellationToken cancellationToken) =>
        new(
            true,
            await _collection.DeleteOneAsync(appUser => appUser.Email == userEmail, cancellationToken)
        );

    public async Task<OperationResult<UpdateResult>> UpdateLastActive(string userIdHashed, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(userIdHashed, cancellationToken);

        if (userId is null)
            return new OperationResult<UpdateResult>();

        UpdateDefinition<AppUser> updatedUserLastActive = Builders<AppUser>.Update
            .Set(appUser => appUser.LastActive, DateTime.UtcNow);

        return new OperationResult<UpdateResult>(
            true,
            await _collection.UpdateOneAsync(appUser => appUser.Id == userId, updatedUserLastActive, null, cancellationToken)
        );
    }

    #endregion CRUD
}