using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Marsey.API
{
    public static class MarseyApi
    {
        private static string _endpoint = "";
        private static bool _enabled = true;
        private static string _releases = "";
        private static List<Version>? _versions = new List<Version>();
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task Initialize(string endpoint, bool enabled)
        {
            _endpoint = endpoint;
            _enabled = enabled;

            await UpdateMarseyVersion();
        }

        public static async Task<bool> MarseyHello()
        {
            return await SendHelloRequest($"{_endpoint}/marsey");
        }

        private static async Task<bool> SendHelloRequest(string url)
        {
            if (!_enabled) return false;
            
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(content);
                    if (json["message"] != null && json["version"] != null)
                    {
                        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Endpoint hello'd correctly!");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MarseyLogger.Log(MarseyLogger.LogType.INFO, $"{url}: MarseyApi excepted! \n{ex.Message}");
            }

            MarseyLogger.Log(MarseyLogger.LogType.INFO, $"{url}: MarseyApi endpoint did not return a proper hello.");
            return false;
        }

        private static async Task UpdateMarseyVersion()
        {
            if (!_enabled) return;
            
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_endpoint}/version");
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(content);
                    if (json["latest"] != null && json["minimum"] != null && json["releases"] != null)
                    {
                        Version latest = new Version(json["latest"]!.ToString());
                        Version minimum = new Version(json["minimum"]!.ToString());
                        _versions = new List<Version> { latest, minimum };
                        _releases = json["releases"]!.ToString();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MarseyLogger.Log(MarseyLogger.LogType.INFO, $"{_endpoint}: MarseyApi excepted! \n{ex.Message}");
            }

            MarseyLogger.Log(MarseyLogger.LogType.INFO, $"{_endpoint}: MarseyApi endpoint did not return a proper version.");
        }

        public static Version? GetLatestVersion() => _versions?[0];
        public static Version? GetMinimumVersion() => _versions?[1];
        public static string? GetReleasesURL() => _releases;
    }
}