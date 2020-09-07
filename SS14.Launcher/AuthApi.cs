using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        public async Task<AuthenticateResult> AuthenticateAsync(string username, string password)
        {
            try
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
                    return new AuthenticateResult(new LoginInfo
                    {
                        UserId = respJson.UserId,
                        Token = respJson.Token,
                        Username = respJson.Username
                    });
                }

                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Login failure.
                    var respJson = await resp.Content.AsJson<AuthenticateDenyResponse>();
                    return new AuthenticateResult(respJson.Errors);
                }

                // Unknown error? uh oh.
                return new AuthenticateResult(new[] {"Server returned unknown error"});
            }
            catch (JsonException)
            {
                return new AuthenticateResult(new[] {"Server sent invalid response"});
            }
            catch (HttpRequestException httpE)
            {
                return new AuthenticateResult(new[] {$"Connection error to authentication server: {httpE.Message}"});
            }
        }

        public async Task<RegisterResult> RegisterAsync(string username, string email, string password)
        {
            try
            {
                var request = new RegisterRequest
                {
                    Username = username,
                    Email = email,
                    Password = password
                };

                const string authUrl = UrlConstants.AuthUrl + "api/auth/register";

                using var resp = await _httpClient.PostAsync(authUrl, request);

                if (resp.IsSuccessStatusCode)
                {
                    var respJson = await resp.Content.AsJson<RegisterResponse>();
                    return new RegisterResult(respJson.Status);
                }

                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Login failure.
                    var respJson = await resp.Content.AsJson<RegisterResponseError>();
                    return new RegisterResult(respJson.Errors);
                }

                // Unknown error? uh oh.
                return new RegisterResult(new[] {"Server returned unknown error"});
            }
            catch (JsonException)
            {
                return new RegisterResult(new[] {"Server sent invalid response"});
            }
            catch (HttpRequestException httpE)
            {
                return new RegisterResult(new[] {$"Connection error to authentication server: {httpE.Message}"});
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

        public sealed class AuthenticateDenyResponse
        {
            public string[] Errors { get; set; } = default!;
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
    }

    public readonly struct AuthenticateResult
    {
        private readonly LoginInfo? _loginInfo;
        private readonly string[]? _errors;

        public AuthenticateResult(LoginInfo loginInfo)
        {
            _loginInfo = loginInfo;
            _errors = null;
        }

        public AuthenticateResult(string[] errors)
        {
            _loginInfo = null;
            _errors = errors;
        }

        public bool IsSuccess => _loginInfo != null;

        public LoginInfo LoginInfo => _loginInfo
                                      ?? throw new InvalidOperationException(
                                          "This AuthenticateResult is not a success.");

        public string[] Errors => _errors
                                  ?? throw new InvalidOperationException("This AuthenticateResult is not a failure.");
    }

    public readonly struct RegisterResult
    {
        private readonly RegisterResponseStatus? _status;
        private readonly string[]? _errors;

        public RegisterResult(RegisterResponseStatus status)
        {
            _status = status;
            _errors = null;
        }

        public RegisterResult(string[] errors)
        {
            _status = null;
            _errors = errors;
        }

        public bool IsSuccess => _status != null;

        public RegisterResponseStatus Status => _status
                                                ?? throw new InvalidOperationException(
                                                    "This RegisterResult is not a success.");

        public string[] Errors => _errors
                                  ?? throw new InvalidOperationException("This RegisterResult is not a failure.");
    }

    public enum RegisterResponseStatus
    {
        Registered,
        RegisteredNeedConfirmation
    }
}
