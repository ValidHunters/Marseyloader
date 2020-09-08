using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Models;

namespace SS14.Launcher.ViewModels.Login
{
    public class RegisterNeedsConfirmationViewModel : ViewModelBase
    {
        private readonly ConfigurationManager _cfg;
        private readonly MainWindowLoginViewModel _parentVm;
        private readonly AuthApi _authApi;
        public const int TimeoutSeconds = 5;

        private (string username, string password)? _loginInfo;

        public bool ConfirmButtonEnabled => TimeoutSecondsLeft == 0;
        public string ConfirmButtonText
        {
            get
            {
                var text = "I have confirmed my account";
                if (TimeoutSecondsLeft != 0)
                {
                    text = $"{text} ({TimeoutSecondsLeft})";
                }

                return text;
            }
        }

        [Reactive] private int TimeoutSecondsLeft { get; set; }

        public RegisterNeedsConfirmationViewModel(ConfigurationManager cfg, MainWindowLoginViewModel parentVm,
            AuthApi authApi)
        {
            _cfg = cfg;
            _parentVm = parentVm;
            _authApi = authApi;
            this.WhenAnyValue(p => p.TimeoutSecondsLeft)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(ConfirmButtonText));
                    this.RaisePropertyChanged(nameof(ConfirmButtonEnabled));
                });
        }

        public void SetLoginInfo(string username, string password)
        {
            _loginInfo = (username, password);
            Selected();
        }

        private void Selected()
        {
            TimeoutSecondsLeft = TimeoutSeconds;
            DispatcherTimer.Run(TimerTick, TimeSpan.FromSeconds(1));
        }

        private bool TimerTick()
        {
            TimeoutSecondsLeft -= 1;
            return TimeoutSecondsLeft != 0;
        }

        public async void ConfirmButtonPressed()
        {
            var (username, password) = _loginInfo!.Value;

            // TODO: Remove Task.Delay here.
            await Task.Delay(1000);
            var resp = await _authApi.AuthenticateAsync(username, password);

            if (resp.IsSuccess)
            {
                var loginInfo = resp.LoginInfo;
                if (_cfg.Logins.Lookup(loginInfo.UserId).HasValue)
                {
                    // Already had a login like this??
                    // TODO: Immediately sign out the token here.
                    _cfg.SelectedLoginId = loginInfo.UserId;
                    _parentVm.Screen = LoginScreen.Login;
                    return;
                }

                _cfg.AddLogin(loginInfo);
                _cfg.SelectedLoginId = loginInfo.UserId;
                _parentVm.Screen = LoginScreen.Login;
            }
            else
            {
                // TODO: Display errors
            }
        }
    }
}
