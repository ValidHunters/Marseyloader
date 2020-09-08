using ReactiveUI;
using SS14.Launcher.Models;
using SS14.Launcher.ViewModels.Login;

namespace SS14.Launcher.ViewModels
{
    public class MainWindowLoginViewModel : ViewModelBase
    {
        private readonly ConfigurationManager _cfg;
        private readonly AuthApi _authApi;
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

        public MainWindowLoginViewModel(ConfigurationManager cfg)
        {
            _cfg = cfg;
            _authApi = new AuthApi(cfg);

            _screen = default!;
            SwitchToLogin();
        }

        public string Version => $"v{LauncherVersion.Version}";

        public void SwitchToLogin()
        {
            Screen = new LoginViewModel(_cfg, this, _authApi);
        }

        public void SwitchToRegister()
        {
            Screen = new RegisterViewModel(_cfg, this, _authApi);
        }

        public void SwitchToForgotPassword()
        {
            Screen = new ForgotPasswordViewModel(_cfg, this);
        }

        public void SwitchToResendConfirmation()
        {
            Screen = new ResendConfirmationViewModel(_cfg, this);
        }

        public void SwitchToRegisterNeedsConfirmation(string username, string password)
        {

        }
    }
}
