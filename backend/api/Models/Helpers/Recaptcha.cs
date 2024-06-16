namespace api.Models.Helpers;

public record Recaptcha(
    bool Success,
    [Optional] DateTime Challenge_ts,  // timestamp of the challenge load (ISO format yyyy-MM-dd'T'HH:mm:ssZZ)
    [Optional] string Hostname,         // the hostname of the site where the reCAPTCHA was solved
    [Optional] List<string> Error_codes
);
