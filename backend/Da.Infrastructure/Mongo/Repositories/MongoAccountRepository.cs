using Da.Domain.Entities;
using Da.Domain.Enums;
using Da.Domain.RepositoryInterfaces;
using Da.Infrastructure.Auth;
using Da.Infrastructure.Mongo.Models;
using Da.Infrastructure.Mongo.Settings;
using Da.Shared.Results;
using Microsoft.AspNetCore.Identity;

namespace Da.Infrastructure.Mongo.Repositories;

public class MongoAccountRepository : IAccountRepository
{
    #region Db and Token Settings

    private readonly IMongoCollection<MongoAppUser> _collectionUsers;
    private readonly IMongoCollection<MongoRefreshToken> _collectionRefreshTokens;

    private readonly JwtSettings _jwtSettings;

    // private readonly IRecaptchaService _recaptchaService; // TODO: Move it
    private readonly UserManager<MongoAppUser> _userManager;
    private readonly ITokenService _tokenService; // save user credential as a token
    private readonly IEmailService _emailService;
    private readonly IPhotoService _photoService;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IUserRepository _userRepository;

    // constructor - dependency injection
    public MongoAccountRepository(
        IMongoClient client, IMyMongoDbSettings dbSettings, IConfiguration config, IHostEnvironment hostEnvironment,
        IRecaptchaService recaptchaValidatorService,
        UserManager<MongoAppUser> userManager, ITokenService tokenService,
        IEmailService emailService, IPhotoService photoService,
        IUserRepository userRepository
    )
    {
        IMongoDatabase dbName = client.GetDatabase(dbSettings.DatabaseName)
                                ?? throw new ArgumentNullException(nameof(dbName), "The database name cannot be null.");
        _collectionUsers = dbName.GetCollection<MongoAppUser>(MongoCollectionNames.Users);
        _collectionRefreshTokens = dbName.GetCollection<MongoRefreshToken>(MongoCollectionNames.RefreshTokens);
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

    public async Task<OperationResult> CreateAsync(AppUser appUser, CancellationToken cancellationToken)
    {
        // if (!await ValidateRecaptcha(registerDto.RecaptchaToken, cancellationToken))
        // {
        //     return new OperationResult(
        //         false,
        //         new CustomError(
        //             ErrorCode.IsRecaptchaTokenInvalid,
        //             RecaptchaErrorMessage
        //         )
        //     );
        // }
        
        MongoAppUser? existingUser = await _userManager.FindByEmailAsync(appUser.Email);
        if (existingUser != null)
            return await RegisterIfEmailAlreadyExists(existingUser, appUser, cancellationToken);

        MongoAppUser mongoAppUser = MongoMappers.MapAppUserToMongoAppUser(appUser);

        IdentityResult userCreatedResult = await _userManager.CreateAsync(mongoAppUser, appUser.Password);
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
            mongoAppUser, AppRolesProvider.GetRoleStrValue(Roles.Member)
        );
        if (!roleResult.Succeeded) // Failed to add the role. Delete appUser from DB
        {
            await _userManager.DeleteAsync(mongoAppUser);
            return new OperationResult(false, null);
        }

        if (!await SendVerificationCode(mongoAppUser, cancellationToken))
            throw new ArgumentException(nameof(mongoAppUser.Email) + ": Failed to email verification code.");

        return new OperationResult(true, null); // Account created successfully.
    }

    public async Task<OperationResult<bool>> UpdateLastActive(
        string loggedInUserIdHashed, CancellationToken cancellationToken
    ) => throw new NotImplementedException();

    #endregion CRUD
}