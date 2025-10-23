using Image_Processing_Blob.Interfaces;
using Image_Processing_Blob.Services;

namespace api.Extensions;

public static class RepositoryServiceExtensions
{
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        services.AddScoped<IRecaptchaService, RecaptchaService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITokenCookieService, TokenCookieService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<IPhotoModifySaveService, PhotoModifySaveService>();
        services.AddScoped<IFollowRepository, FollowRepository>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();

        return services;
    }
}