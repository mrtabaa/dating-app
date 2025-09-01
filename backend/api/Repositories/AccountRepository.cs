using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;
using IdentityResult = Microsoft.AspNetCore.Identity.IdentityResult;

namespace api.Repositories;

public class AccountRepository : IAccountRepository
{
    #region ErrorMessages

    private const string RecaptchaErrorMessage = "Recaptcha token is invalid. 'Slide me!' again.";
    private const string WrongCredsErrorMessage = "Wrong username or password.";
    private const string SessionExpiredMessage = "Your session has expired. Login again.";

    private const string EmailAlreadyConfirmedErrorMessage = "This email is already registered and verified. " +
                                                             "You can login or recover your password if the owner.";

    #endregion

    #region Helpers

    private async Task<OperationResult> RegisterIfEmailAlreadyExists(
        AppUser existingUser, RegisterDto registerDto, CancellationToken cancellationToken
    )
    {
        if (await _userManager.IsEmailConfirmedAsync(existingUser))
        {
            return new OperationResult(
                false,
                new CustomError(
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
            return new OperationResult(
                false,
                new CustomError(
                    ErrorCode.NetIdentityFailed,
                    updateResult.Errors.FirstOrDefault()?.Description
                )
            );
        }

        IdentityResult roleResult = await _userManager.AddToRoleAsync(
            existingUser, EnumExtensions.GetRoleStrValue(Roles.Member)
        );
        if (!roleResult.Succeeded) // Failed to add the role. Delete appUser from DB
        {
            await _userManager.DeleteAsync(existingUser);
            return new OperationResult(false, null);
        }

        // Resend the verification email
        if (!await SendVerificationCode(existingUser, cancellationToken))
            throw new ArgumentException(nameof(existingUser.Email) + ": Failed to email verification code.");

        return new OperationResult(true, null); // Account created successfully.
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
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("Base URL cannot be null or empty.", nameof(baseUrl));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(resetToken))
            throw new ArgumentException("Token cannot be null or empty.", nameof(resetToken));

        var builder = new UriBuilder(baseUrl);
        NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);

        query.Add("email", email);
        query.Add("resetToken", resetToken);
        builder.Query = query.ToString();

        return builder.Uri.ToString();
    }

    private static bool ValidateSessionMetadata(SessionMetadata? sessionMetadata, RefreshToken refreshToken) =>
        (refreshToken.SessionMetadata is null || sessionMetadata is null)
            ? throw new ArgumentNullException(nameof(sessionMetadata), "sessionMetadata cannot be null")
            : ValidationsExtension.ValidateObjectId(refreshToken.UserId)
              && refreshToken.SessionMetadata.DeviceType == sessionMetadata.DeviceType
              && refreshToken.SessionMetadata.DeviceName == sessionMetadata.DeviceName
              && refreshToken.SessionMetadata.UserAgent == sessionMetadata.UserAgent
              && refreshToken.SessionMetadata.IpAddress == sessionMetadata.IpAddress;


    private async Task RevokeAllRefreshTokensAsync(ObjectId userId, CancellationToken cancellationToken)
    {
        // TODO: Record and warn as hacked / Token stolen and used again.
        UpdateDefinition<RefreshToken> updateDefinition = Builders<RefreshToken>.Update.Set(
            token => token.IsRevoked, true
        );

        await _collectionRefreshTokens.UpdateManyAsync(
            token => token.UserId == userId, updateDefinition, null, cancellationToken
        );
    }

    #endregion

    #region Db and Token Settings

    private readonly IMongoCollection<AppUser> _collectionUsers;
    private readonly IMongoCollection<RefreshToken> _collectionRefreshTokens;
    private readonly JwtSettings _jwtSettings;
    private readonly IRecaptchaService _recaptchaService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService; // save user credential as a token
    private readonly IEmailService _emailService;
    private readonly IPhotoService _photoService;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IUserRepository _userRepository;

    // constructor - dependency injection
    public AccountRepository(
        IMongoClient client, IMyMongoDbSettings dbSettings, IConfiguration config, IHostEnvironment hostEnvironment,
        IRecaptchaService recaptchaValidatorService,
        UserManager<AppUser> userManager, ITokenService tokenService,
        IEmailService emailService, IPhotoService photoService,
        IUserRepository userRepository
    )
    {
        IMongoDatabase dbName = client.GetDatabase(dbSettings.DatabaseName)
                                ?? throw new ArgumentNullException(nameof(dbName), "The database name cannot be null.");
        _collectionUsers = dbName.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);
        _collectionRefreshTokens = dbName.GetCollection<RefreshToken>(AppVariablesExtensions.CollectionRefreshTokens);
        _jwtSettings = config.GetSection(nameof(JwtSettings)).Get<JwtSettings>()
                       ?? throw new ArgumentNullException(nameof(JwtSettings));
        _recaptchaService = recaptchaValidatorService;
        _userManager = userManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _photoService = photoService;
        _hostEnvironment = hostEnvironment;
        _userRepository = userRepository;
    }

    #endregion

    #region CRUD

    public async Task<OperationResult> CreateAsync(RegisterDto registerDto, CancellationToken cancellationToken)
    {
        if (!await ValidateRecaptcha(registerDto.RecaptchaToken, cancellationToken))
        {
            return new OperationResult(
                false,
                new CustomError(
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
            return new OperationResult(
                false,
                new CustomError(
                    ErrorCode.NetIdentityFailed,
                    userCreatedResult.Errors.Select(e => e.Description).FirstOrDefault()
                )
            );
        }

        IdentityResult roleResult = await _userManager.AddToRoleAsync(
            appUser, EnumExtensions.GetRoleStrValue(Roles.Member)
        );
        if (!roleResult.Succeeded) // Failed to add the role. Delete appUser from DB
        {
            await _userManager.DeleteAsync(appUser);
            return new OperationResult(false, null);
        }

        if (!await SendVerificationCode(appUser, cancellationToken))
            throw new ArgumentException(nameof(appUser.Email) + ": Failed to email verification code.");

        return new OperationResult(true, null); // Account created successfully.
    }

    public async Task<OperationResult<LoginResult>> VerifyAsync(
        VerifyDto verifyDto, SessionMetadata sessionMetadata, CancellationToken cancellationToken
    )
    {
        AppUser? appUser = await _userManager.FindByEmailAsync(verifyDto.Email);
        if (appUser is null)
            return new OperationResult<LoginResult>(false, Error: null);

        if (await _userManager.IsEmailConfirmedAsync(appUser))
        {
            return new OperationResult<LoginResult>(
                false,
                Error: new CustomError(
                    ErrorCode.IsEmailAlreadyConfirmed,
                    EmailAlreadyConfirmedErrorMessage
                )
            );
        }

        IdentityResult result = await _userManager.ConfirmEmailAsync(appUser, verifyDto.Code);
        if (!result.Succeeded)
            return new OperationResult<LoginResult>(false, Error: null);

        RefreshTokenRequest refreshTokenRequest = new()
        {
            JtiValue = Guid.CreateVersion7().ToString(),
            SessionMetadata = sessionMetadata
        };

        return new OperationResult<LoginResult>(
            true,
            new LoginResult(
                Mappers.ConvertAppUserToLoggedInDto(
                    appUser, await _userManager.GetRolesAsync(appUser), GetMainPhoto(appUser)
                ),
                await _tokenService.GenerateTokensAsync(refreshTokenRequest, appUser, cancellationToken)
            ),
            null
        );
    }

    public async Task<OperationResult> ResendVerifyCodeAsync(
        ResendCodeRequest resendCRequest, CancellationToken cancellationToken
    )
    {
        if (!await ValidateRecaptcha(resendCRequest.RecaptchaToken, cancellationToken))
        {
            return new OperationResult(
                false,
                new CustomError(
                    ErrorCode.IsRecaptchaTokenInvalid,
                    RecaptchaErrorMessage
                )
            );
        }

        AppUser? appUser = await _userManager.FindByEmailAsync(resendCRequest.Email);
        if (appUser is null) return new OperationResult(false, null);

        if (await _userManager.IsEmailConfirmedAsync(appUser))
        {
            return new OperationResult(
                false,
                new CustomError(
                    ErrorCode.IsEmailAlreadyConfirmed,
                    EmailAlreadyConfirmedErrorMessage
                )
            );
        }

        return new OperationResult(
            await SendVerificationCode(appUser, cancellationToken),
            null
        ); // Success depends on the email sent success
    }

    public async Task<OperationResult<LoginResult>> LoginAsync(
        LoginDto userInput, SessionMetadata sessionMetadata, CancellationToken cancellationToken
    )
    {
        if (!await ValidateRecaptcha(userInput.RecaptchaToken, cancellationToken))
        {
            return new OperationResult<LoginResult>(
                false,
                Error: new CustomError(
                    ErrorCode.IsRecaptchaTokenInvalid,
                    RecaptchaErrorMessage
                )
            );
        }

        AppUser? appUser;

        #region Check credentials

        // Find appUser by Email or UserName
        if (Regex.IsMatch(userInput.EmailUsername, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$")) // If input is email
            appUser = await _userManager.FindByEmailAsync(userInput.EmailUsername);
        else
            appUser = await _userManager.FindByNameAsync(userInput.EmailUsername);

        if (appUser is null)
        {
            return new OperationResult<LoginResult>(
                false,
                Error: new CustomError(
                    ErrorCode.IsWrongCreds,
                    WrongCredsErrorMessage
                )
            );
        }

        if (!await _userManager.CheckPasswordAsync(appUser, userInput.Password))
        {
            return new OperationResult<LoginResult>(
                false,
                Error: new CustomError(
                    ErrorCode.IsWrongCreds,
                    WrongCredsErrorMessage
                )
            );
        }

        #endregion

        if (!await _userManager.IsEmailConfirmedAsync(appUser))
        {
            if (!await SendVerificationCode(appUser, cancellationToken))
                throw new ArgumentException(nameof(appUser.UserName) + ": Failed to email the verification code.");

            return new OperationResult<LoginResult>(
                false,
                new LoginResult(
                    new LoggedInDto(Email: appUser.Email?.ToLower(), IsEmailNotConfirmed: true)
                ),
                new CustomError(
                    ErrorCode.IsEmailNotConfirmed
                )
            );
        }

        RefreshTokenRequest refreshTokenRequest = new()
        {
            JtiValue = Guid.CreateVersion7().ToString(),
            SessionMetadata = sessionMetadata
        };

        return new OperationResult<LoginResult>(
            true,
            new LoginResult(
                Mappers.ConvertAppUserToLoggedInDto(
                    appUser, await _userManager.GetRolesAsync(appUser), GetMainPhoto(appUser)
                ),
                await _tokenService.GenerateTokensAsync(refreshTokenRequest, appUser, cancellationToken)
            ),
            null
        );
    }

    public async Task<OperationResult<TokenDto>> RefreshTokensAsync(
        RefreshTokenRequest refreshTokenRequest, CancellationToken cancellationToken
    )
    {
        RefreshToken expectedTokenFromDb = await _collectionRefreshTokens.
                                               Find(
                                                   token => token.JtiValue == refreshTokenRequest.JtiValue
                                               ).
                                               SingleOrDefaultAsync(cancellationToken)
                                           ?? throw new ArgumentNullException(
                                               nameof(expectedTokenFromDb), "cannot be null."
                                           );

        #region Hacked

        if (!ValidateSessionMetadata(refreshTokenRequest.SessionMetadata, expectedTokenFromDb))
        {
            //TODO: Logged in from new device/location. Perform 2FA.
        }

        if (expectedTokenFromDb.UsedAt is not null)
        {
            await RevokeAllRefreshTokensAsync(expectedTokenFromDb.UserId, cancellationToken);
            // TODO: Hacked, force to change password.    
        }

        #endregion

        bool isInvalid = !TokenHasher.ValidateToken(
            TokenHasher.HashWithSecret(refreshTokenRequest.TokenValueRaw, _jwtSettings.TokenKey), 
            expectedTokenFromDb.TokenValueHashed
        );

        // Check token expiration, revoked, and HMAC validation
        if (
            expectedTokenFromDb.ExpiresAt < DateTimeOffset.UtcNow
            || expectedTokenFromDb.IsRevoked // e.g. revoked by admin or user logout
            || isInvalid
        )
        {
            return new OperationResult<TokenDto>(
                false,
                Error: new CustomError(
                    ErrorCode.IsSessionExpired,
                    SessionExpiredMessage
                )
            );
        }

        UpdateDefinition<RefreshToken>? revokeTokenUpdateDef = Builders<RefreshToken>.Update.
            Set(token => token.UsedAt, DateTimeOffset.UtcNow).Set(t => t.IsRevoked, true);

        await _collectionRefreshTokens.UpdateOneAsync(
            token => token.Id == expectedTokenFromDb.Id, revokeTokenUpdateDef, null,
            cancellationToken
        );

        AppUser? appUser = await _userRepository.GetByIdAsync(expectedTokenFromDb.UserId, cancellationToken);
        if (appUser is null)
        {
            return new OperationResult<TokenDto>(
                false,
                Error: new CustomError(
                    ErrorCode.IsSessionExpired,
                    SessionExpiredMessage
                )
            );
        }

        // Token is valid
        return new OperationResult<TokenDto>(
            true,
            await _tokenService.GenerateTokensAsync(refreshTokenRequest, appUser, cancellationToken),
            null
        );
    }

    public async Task<OperationResult<LoggedInDto>> ReloadLoggedInUserAsync(
        string userIdHashed, CancellationToken cancellationToken
    )
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(userIdHashed, cancellationToken);

        if (userId is null)
            return new OperationResult<LoggedInDto>(false, Error: null);

        AppUser appUser = await _collectionUsers.Find(appUser => appUser.Id == userId).
            FirstOrDefaultAsync(cancellationToken);

        return appUser is null
            ? new OperationResult<LoggedInDto>(false, Error: null)
            : new OperationResult<LoggedInDto>(
                true,
                Mappers.ConvertAppUserToLoggedInDto(
                    appUser, await _userManager.GetRolesAsync(appUser), GetMainPhoto(appUser)
                ),
                null
            );
    }

    public async Task<OperationResult> RequestResetPasswordAsync(
        ResetPasswordRequest request, CancellationToken cancellationToken
    )
    {
        if (!await ValidateRecaptcha(request.RecaptchaToken, cancellationToken))
        {
            return new OperationResult(
                false,
                new CustomError(
                    ErrorCode.IsRecaptchaTokenInvalid,
                    RecaptchaErrorMessage
                )
            );
        }

        AppUser? appUser = await _userManager.FindByEmailAsync(request.Email.Trim());

        if (appUser is null || string.IsNullOrEmpty(appUser.Email))
            return new OperationResult(false, null);

        string resetToken = await _userManager.GeneratePasswordResetTokenAsync(appUser);

        string resetLink = GenerateResetPasswordLink(
            // "http://localhost:4300/account/reset-password",
            "https://www.hallboard.com/account/reset-password",
            appUser.Email, resetToken
        );

        if (!await _emailService.SendPasswordResetLink(appUser, resetLink, cancellationToken))
        {
            throw new ArgumentException(
                "Failed to send reset password link. Check if email provider is working.", nameof(appUser.Email)
            );
        }

        return new OperationResult(false, null);
    }

    public async Task<OperationResult> ResetPasswordAsync(
        ResetPassword resetPassword, CancellationToken cancellationToken
    )
    {
        AppUser? appUser = await _userManager.FindByEmailAsync(resetPassword.Email.Trim());
        if (appUser is null)
            return new OperationResult(false, null);

        IdentityResult passwordResetResult = await _userManager.ResetPasswordAsync(
            appUser, resetPassword.ResetToken, resetPassword.Password
        );

        if (!passwordResetResult.Succeeded) return new OperationResult(false, null);
        if (!await _emailService.SendResetPasswordConfirmation(appUser))
        {
            throw new ArgumentException(
                "Failed to send reset password confirmation. Check if email provider is working.", nameof(appUser.Email)
            );
        }

        return new OperationResult(passwordResetResult.Succeeded, null);
    }

    public async Task<OperationResult<DeleteResult>> DeleteUserAsync(
        string? userEmail, CancellationToken cancellationToken
    ) =>
        new(
            true,
            await _collectionUsers.DeleteOneAsync(appUser => appUser.Email == userEmail, cancellationToken),
            null
        );

    public async Task<OperationResult<UpdateResult>> UpdateLastActive(
        string userIdHashed, CancellationToken cancellationToken
    )
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(userIdHashed, cancellationToken);

        if (userId is null)
            return new OperationResult<UpdateResult>(false, Error: null);

        UpdateDefinition<AppUser> updatedUserLastActive = Builders<AppUser>.Update.Set(
            appUser => appUser.LastActive, DateTime.UtcNow
        );

        return new OperationResult<UpdateResult>(
            true,
            await _collectionUsers.UpdateOneAsync(
                appUser => appUser.Id == userId, updatedUserLastActive, null, cancellationToken
            ),
            null
        );
    }

    #endregion CRUD
}