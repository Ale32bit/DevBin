using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DevBin.Services
{
    public class AbuseIPDB
    {
        public int Treshold = 25;
        private readonly string _apiToken;
        private readonly HttpClient client;
        private readonly Uri BaseAddress = new Uri("https://api.abuseipdb.com/api/v2/");
        private readonly bool _enabled;
        public AbuseIPDB(string apiToken)
        {
            _apiToken = apiToken;
            _enabled = string.IsNullOrWhiteSpace(apiToken);
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Key", _apiToken);
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        }

        public async Task<bool> CheckIP(IPAddress ipAddress)
        {

            if (!_enabled) return true;

            var values = new Dictionary<string, string> {
                { "ipAddress", ipAddress.ToString() },
                { "maxAgeInDays", "90" },
                { "verbose", "" }
            };
            var query = QueryString.Create(values);
            var url = new Uri(BaseAddress, "check" + query.ToUriComponent());
            var result = await client.GetAsync(url);

            result.EnsureSuccessStatusCode();

            var response = await result.Content.ReadAsStringAsync();

            dynamic data = JsonConvert.DeserializeObject(response);
            dynamic value = data.data.abuseConfidenceScore.Value;
            int score = (int)value;

            return score < Treshold;
        }
    }
}
