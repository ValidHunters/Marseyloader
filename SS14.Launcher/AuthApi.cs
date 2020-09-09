using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using SS14.Launcher.Models;

namespace SS14.Launcher
{
    public sealed class AuthApi
    {
        private readonly DataManager _config;
        private readonly HttpClient _httpClient;

        public AuthApi(DataManager config)
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

                const string authUrl = ConfigConstants.AuthUrl + "api/auth/authenticate";

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

                Log.Error("Server returned unexpected HTTP status code: {responseCode}", resp.StatusCode);
                Log.Debug("Response for error:\n{response}\n{content}", resp, await resp.Content.ReadAsStringAsync());
                // Unknown error? uh oh.
                return new AuthenticateResult(new[] {"Server returned unknown error"});
            }
            catch (JsonException e)
            {
                Log.Error(e, "JsonException in AuthenticateAsync");
                return new AuthenticateResult(new[] {"Server sent invalid response"});
            }
            catch (HttpRequestException httpE)
            {
                Log.Error(httpE, "HttpRequestException in AuthenticateAsync");
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

                const string authUrl = ConfigConstants.AuthUrl + "api/auth/register";

                using var resp = await _httpClient.PostAsync(authUrl, request);

                if (resp.IsSuccessStatusCode)
                {
                    var respJson = await resp.Content.AsJson<RegisterResponse>();
                    return new RegisterResult(respJson.Status);
                }

                if (resp.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    // Register failure.
                    var respJson = await resp.Content.AsJson<RegisterResponseError>();
                    return new RegisterResult(respJson.Errors);
                }

                Log.Error("Server returned unexpected HTTP status code: {responseCode}", resp.StatusCode);
                Log.Debug("Response for error:\n{response}\n{content}", resp, await resp.Content.ReadAsStringAsync());
                // Unknown error? uh oh.
                return new RegisterResult(new[] {"Server returned unknown error"});
            }
            catch (JsonException e)
            {
                Log.Error(e, "JsonException in RegisterAsync");
                return new RegisterResult(new[] {"Server sent invalid response"});
            }
            catch (HttpRequestException httpE)
            {
                Log.Error(httpE, "HttpRequestException in RegisterAsync");
                return new RegisterResult(new[] {$"Connection error to authentication server: {httpE.Message}"});
            }
        }

        /// <returns>Any errors that occured</returns>
        public async Task<string[]?> ForgotPasswordAsync(string email)
        {
            try
            {
                var request = new ResetPasswordRequest
                {
                    Email = email,
                };

                const string authUrl = ConfigConstants.AuthUrl + "api/auth/resetPassword";

                using var resp = await _httpClient.PostAsync(authUrl, request);

                if (resp.IsSuccessStatusCode)
                {
                    return null;
                }

                // Unknown error? uh oh.
                Log.Error("Server returned unexpected HTTP status code: {responseCode}", resp.StatusCode);
                Log.Debug("Response for error:\n{response}\n{content}", resp, await resp.Content.ReadAsStringAsync());
                return new[] {"Server returned unknown error"};
            }
            catch (HttpRequestException httpE)
            {
                Log.Error(httpE, "HttpRequestException in ForgotPasswordAsync");
                return new[] {$"Connection error to authentication server: {httpE.Message}"};
            }
        }

        public async Task<string[]?> ResendConfirmationAsync(string email)
        {
            try
            {
                var request = new ResendConfirmationRequest
                {
                    Email = email,
                };

                const string authUrl = ConfigConstants.AuthUrl + "api/auth/resendConfirmation";

                using var resp = await _httpClient.PostAsync(authUrl, request);

                if (resp.IsSuccessStatusCode)
                {
                    return null;
                }

                // Unknown error? uh oh.
                Log.Error("Server returned unexpected HTTP status code: {responseCode}", resp.StatusCode);
                Log.Debug("Response for error:\n{response}\n{content}", resp, await resp.Content.ReadAsStringAsync());
                return new[] {"Server returned unknown error"};
            }
            catch (HttpRequestException httpE)
            {
                Log.Error(httpE, "HttpRequestException in ResendConfirmationAsync");
                return new[] {$"Connection error to authentication server: {httpE.Message}"};
            }
        }

        public async Task LogoutTokenAsync(string token)
        {
            try
            {
                var request = new LogoutRequest
                {
                    Token = token
                };

                const string authUrl = ConfigConstants.AuthUrl + "api/auth/logout";

                using var resp = await _httpClient.PostAsync(authUrl, request);

                if (resp.IsSuccessStatusCode)
                {
                    return;
                }

                // Unknown error? uh oh.
                Log.Error("Server returned unexpected HTTP status code: {responseCode}", resp.StatusCode);
                Log.Debug("Response for error:\n{response}\n{content}", resp, await resp.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException httpE)
            {
                // Does it make sense to just... swallow this exception? The token will stay "active" until it expires.
                Log.Error(httpE, "HttpRequestException in LogoutTokenAsync");
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

        public sealed class ResetPasswordRequest
        {
            public string Email { get; set; } = default!;
        }

        public sealed class ResendConfirmationRequest
        {
            public string Email { get; set; } = default!;
        }

        public sealed class LogoutRequest
        {
            public string Token { get; set; } = default!;
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
