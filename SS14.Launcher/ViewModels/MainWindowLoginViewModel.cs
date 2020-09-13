using ReactiveUI;
using SS14.Launcher.Models;
using SS14.Launcher.Models.Logins;
using SS14.Launcher.ViewModels.Login;

namespace SS14.Launcher.ViewModels
{
    public class MainWindowLoginViewModel : ViewModelBase
    {
        private readonly DataManager _cfg;
        private readonly AuthApi _authApi;
        private readonly LoginManager _loginMgr;
        private BaseLoginViewModel _screen;

        public BaseLoginViewModel Screen
        {
            get => _screen;
            set
            {
                this.RaiseAndSetIfChanged(ref _screen, value);
                value.Activated();
            }
        }

        public MainWindowLoginViewModel(DataManager cfg, AuthApi authApi, LoginManager loginMgr)
        {
            _cfg = cfg;
            _authApi = authApi;
            _loginMgr = loginMgr;

            _screen = default!;
            SwitchToLogin();
        }

        public string Version => $"v{LauncherVersion.Version}";

        public void SwitchToLogin()
        {
            Screen = new LoginViewModel(_cfg, this, _authApi, _loginMgr);
        }

        public void SwitchToExpiredLogin(LoggedInAccount account)
        {
            Screen = new ExpiredLoginViewModel(_cfg, this, _authApi, _loginMgr, account);
        }

        public void SwitchToRegister()
        {
            Screen = new RegisterViewModel(_cfg, this, _authApi, _loginMgr);
        }

        public void SwitchToForgotPassword()
        {
            Screen = new ForgotPasswordViewModel(_cfg, this, _authApi);
        }

        public void SwitchToResendConfirmation()
        {
            Screen = new ResendConfirmationViewModel(this, _authApi);
        }

        public void SwitchToRegisterNeedsConfirmation(string username, string password)
        {
            Screen = new RegisterNeedsConfirmationViewModel(_cfg, this, _authApi, username, password, _loginMgr);
        }
    }
}
