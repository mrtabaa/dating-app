namespace da.Application.DTOs.Account;

public enum ErrorCode
{
    IsRecaptchaTokenInvalid,
    IsEmailAlreadyConfirmed,
    IsWrongCreds,
    IsSessionExpired,
    NetIdentityFailed,
    IsEmailNotConfirmed
}