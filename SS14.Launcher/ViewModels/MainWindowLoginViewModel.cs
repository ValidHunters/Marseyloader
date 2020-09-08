using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Models;
using SS14.Launcher.ViewModels.Login;

namespace SS14.Launcher.ViewModels
{
    public class MainWindowLoginViewModel : ViewModelBase
    {
        public LoginViewModel Login { get; }
        public RegisterViewModel Register { get; }
        public RegisterNeedsConfirmationViewModel RegisterNeedsConfirmation { get; }
        public ForgotPasswordViewModel ForgotPassword { get; }
        public ResendConfirmationViewModel ResendConfirmation { get; }

        public bool ScreenLogin => Screen == LoginScreen.Login;
        public bool ScreenRegister => Screen == LoginScreen.Register;
        public bool ScreenRegisterNeedsConfirmation => Screen == LoginScreen.RegisterNeedsConfirmation;
        public bool ScreenForgotPassword => Screen == LoginScreen.ForgotPassword;
        public bool ScreenResendConfirmation => Screen == LoginScreen.ResendConfirmation;

        [Reactive] public LoginScreen Screen { get; set; }

        public MainWindowLoginViewModel(ConfigurationManager cfg)
        {
            var authApi = new AuthApi(cfg);

            Login = new LoginViewModel(cfg, this, authApi);
            Register = new RegisterViewModel(cfg, this, authApi);
            RegisterNeedsConfirmation = new RegisterNeedsConfirmationViewModel(cfg, this, authApi);
            ForgotPassword = new ForgotPasswordViewModel(cfg, this);
            ResendConfirmation = new ResendConfirmationViewModel(cfg, this);

            this.WhenAnyValue(p => p.Screen)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(ScreenLogin));
                    this.RaisePropertyChanged(nameof(ScreenRegister));
                    this.RaisePropertyChanged(nameof(ScreenRegisterNeedsConfirmation));
                    this.RaisePropertyChanged(nameof(ScreenForgotPassword));
                    this.RaisePropertyChanged(nameof(ScreenResendConfirmation));
                });
        }

        public string Version => $"v{LauncherVersion.Version}";
    }

    public enum LoginScreen
    {
        Login,
        Register,
        RegisterNeedsConfirmation,
        ForgotPassword,
        ResendConfirmation
    }
}
