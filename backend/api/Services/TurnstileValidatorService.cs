using Newtonsoft.Json;

namespace api.Services;

public class TurnstileValidatorService(HttpClient _httpClient, IConfiguration _config) : ITurnstileValidatorService
{
    public async Task<bool> ValidateTokenAsync(string turnstileToken, CancellationToken cancellationToken)
    {
        string? secretKey = _config.GetValue<string>("TurnstileSecretKey");

        if (string.IsNullOrEmpty(secretKey)) return false;

        var values = new Dictionary<string, string>
        {
            {"secret", secretKey},
            {"response", turnstileToken},
        };

        var content = new FormUrlEncodedContent(values);
        HttpResponseMessage? response = await _httpClient.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", content, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            string responseString = await response.Content.ReadAsStringAsync();

            dynamic? jsonResponse = JsonConvert.DeserializeObject(responseString);

            if (jsonResponse != null)
                return jsonResponse.success;
        }

        return false;
    }
}
