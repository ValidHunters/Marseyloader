using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Marsey.Utility;
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

        private static void Log(MarseyLogger.LogType type, string message)
        {
            MarseyLogger.Log(type, message);
        }

        private static async Task<JObject?> GetJsonResponse(string url)
        {
            if (!_enabled) return null;
    
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    return JObject.Parse(content);
                }
            }
            catch (Exception ex)
            {
                Log(MarseyLogger.LogType.INFO, $"{url}: MarseyApi excepted! \n{ex.Message}");
            }

            Log(MarseyLogger.LogType.INFO, $"{url}: Endpoint did not return a proper response.");
            return null;
        }

        private static async Task<bool> SendHelloRequest(string url)
        {
            JObject? json = await GetJsonResponse(url);
            
            if (json?["message"] == null || json?["version"] == null) return false;
            
            Log(MarseyLogger.LogType.DEBG, "Endpoint hello'd correctly!");
            return true;
        }

        private static async Task UpdateMarseyVersion()
        {
            JObject? json = await GetJsonResponse($"{_endpoint}/version");
            if (json?["latest"] != null && json?["minimum"] != null && json?["releases"] != null)
            {
                Version latest = new Version(json["latest"]!.ToString());
                Version minimum = new Version(json["minimum"]!.ToString());
                _versions = new List<Version> { latest, minimum };
                _releases = json["releases"]!.ToString();
            }
        }


        public static Version? GetLatestVersion() => _versions?[0];
        public static Version? GetMinimumVersion() => _versions?[1];
        public static string? GetReleasesURL() => _releases;
    }
}