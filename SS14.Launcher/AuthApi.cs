using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SS14.Launcher.Models;

namespace SS14.Launcher
{
    public sealed class AuthApi
    {
        private readonly ConfigurationManager _config;
        private readonly HttpClient _httpClient;

        public AuthApi(ConfigurationManager config)
        {
            _config = config;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue(LauncherVersion.Name, LauncherVersion.Version?.ToString()));

            _httpClient.DefaultRequestHeaders.Add("SS14-Launcher-Fingerprint", _config.Fingerprint.ToString());
        }

        public async Task<object> AuthenticateAsync(string username, string password)
        {
            var request = new AuthenticateRequest
            {
                Username = username,
                Password = password
            };

            const string authUrl = UrlConstants.AuthUrl + "api/auth/authenticate";

            using var resp = await _httpClient.PostAsync(authUrl, request);

            if (resp.IsSuccessStatusCode)
            {
                var respJson = await resp.Content.AsJson<AuthenticateResponse>();
                return new LoginInfo
                {
                    UserId = respJson.UserId,
                    Token = respJson.Token,
                    Username = respJson.Username
                };
            }

            throw new NotImplementedException();
        }
    }

    public sealed class AuthenticateRequest
    {
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public sealed class AuthenticateResponse
    {
        public string Token { get; set; } = default!;
        public string Username { get; set; } = default!;
        public Guid UserId { get; set; }
    }

    public sealed class RegisterRequest
    {
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public sealed class RegisterResponse
    {
        public RegisterResponseStatus Status { get; set; }
    }

    public sealed class RegisterResponseError
    {
        public string[] Errors { get; set; } = default!;
    }

    public enum RegisterResponseStatus
    {
        Registered,
        RegisteredNeedConfirmation
    }
}
