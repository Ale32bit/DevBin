using Newtonsoft.Json;

namespace DevBin.Services.HCaptcha;

public class Result
{
    [JsonProperty("success")]
    public bool Success { get; set; }
    [JsonProperty("challenge_ts")]
    public DateTime ChallengeTS { get; set; }
    [JsonProperty("hostname")]
    public string Hostname { get; set; }
    [JsonProperty("credit")]
    public bool Credit { get; set; }
    [JsonProperty("error-codes")]
    public object[] ErrorCodes { get; set; }
}