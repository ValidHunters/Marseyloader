namespace SS14.Launcher.Models
{
    public sealed class LoginManager
    {
        private readonly DataManager _cfg;
        private readonly AuthApi _authApi;

        public LoginManager(DataManager cfg, AuthApi authApi)
        {
            _cfg = cfg;
            _authApi = authApi;
        }
    }
}
