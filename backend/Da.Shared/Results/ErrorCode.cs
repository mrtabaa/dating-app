namespace Da.Shared.Results;

public enum ErrorCode
{
    IsRecaptchaTokenInvalid,
    IsEmailAlreadyConfirmed,
    IsWrongCreds,
    IsSessionExpired,
    NetIdentityFailed,
    IsEmailNotConfirmed
}