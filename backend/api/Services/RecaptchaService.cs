using Newtonsoft.Json;

namespace api.Services;

public class RecaptchaService(HttpClient httpClient, IConfiguration config) : IRecaptchaService
{
    private IConfiguration _config = config;
    private HttpClient _httpClient = httpClient;

    public async Task<bool> ValidateTokenAsync(string? recaptchaToken, CancellationToken cancellationToken)
    {
        _httpClient = _httpClient ?? throw new ArgumentNullException(nameof(httpClient), "httpClient should not be null");
        _config = _config ?? throw new ArgumentNullException(nameof(config), "config should not be null");

        if (string.IsNullOrEmpty(recaptchaToken)) return false;

        var secretKey = _config.GetValue<string>(AppVariablesExtensions.RecaptchaSecretKey);

        if (string.IsNullOrEmpty(secretKey)) return false;

        var values = new Dictionary<string, string>
        {
            { "secret", secretKey },
            { "response", recaptchaToken }
        };

        var content = new FormUrlEncodedContent(values);

        HttpResponseMessage response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            string responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            var jsonResponse = JsonConvert.DeserializeObject<Recaptcha>(responseString);

            if (jsonResponse != null) return jsonResponse.Success;
        }

        return false;
    }
}