using Newtonsoft.Json;
using System.Net;

namespace DevBin.Services.HCaptcha;

public class HCaptcha
{
    public Uri Endpoint { get; set; } = new("https://hcaptcha.com/siteverify");
    private readonly HCaptchaOptions _options;
    private readonly HttpClient _client = new();

    public HCaptcha(HCaptchaOptions options)
    {
        _options = options;
    }

    public async Task<bool> VerifyAsync(string? responseToken, IPAddress? address = null)
    {
        if (string.IsNullOrEmpty(responseToken))
            return false;

        var body = new List<KeyValuePair<string, string>> {
            new("secret", _options.SecretKey),
            new("response", responseToken),
            new("sitekey", _options.SiteKey)
        };

        if (address != null)
        {
            body.Add(new("remoteip", address.ToString()));
        }

        var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = new FormUrlEncodedContent(body)
        };

        var result = await _client.SendAsync(request);
        result.EnsureSuccessStatusCode();

        var res = JsonConvert.DeserializeObject<Result>(await result.Content.ReadAsStringAsync());
        return res.Success;
    }
}