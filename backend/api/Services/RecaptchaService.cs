using Newtonsoft.Json;

namespace api.Services;

public class RecaptchaService(HttpClient _httpClient, IConfiguration _config) : IRecaptchaService
{
    public async Task<bool> ValidateTokenAsync(string? recaptchaToken, CancellationToken cancellationToken)
    {
        _httpClient = _httpClient ?? throw new ArgumentNullException(nameof(_httpClient));
        _config = _config ?? throw new ArgumentNullException(nameof(_config));

        if (string.IsNullOrEmpty(recaptchaToken)) return false;

        string? secretKey = _config.GetValue<string>(AppVariablesExtensions.recaptchaSecretKey);

        if (string.IsNullOrEmpty(secretKey)) return false;

        var values = new Dictionary<string, string>
        {
            {"secret", secretKey},
            {"response", recaptchaToken},
        };

        var content = new FormUrlEncodedContent(values);

        HttpResponseMessage? response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            string responseString = await response.Content.ReadAsStringAsync();

            Recaptcha? jsonResponse = JsonConvert.DeserializeObject<Recaptcha>(responseString);

            if (jsonResponse != null) return jsonResponse.Success;
        }

        return false;
    }
}
