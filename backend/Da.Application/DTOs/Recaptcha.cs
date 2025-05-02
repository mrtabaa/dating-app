namespace Da.Application.DTOs;

public record Recaptcha(
    bool Success,
    [Optional] DateTime ChallengeTs, // timestamp of the challenge load (ISO format yyyy-MM-dd'T'HH:mm:ssZZ)
    [Optional] string Hostname, // the hostname of the site where the reCAPTCHA was solved
    [Optional] List<string> ErrorCodes
);