using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Models;

namespace SS14.Launcher.ViewModels.Login
{
    public class RegisterNeedsConfirmationViewModel : BaseLoginViewModel
    {
        private const int TimeoutSeconds = 5;

        private readonly ConfigurationManager _cfg;
        private readonly MainWindowLoginViewModel _parentVm;
        private readonly AuthApi _authApi;

        private readonly string _loginUsername;
        private readonly string _loginPassword;

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
            AuthApi authApi, string username, string password)
        {
            _cfg = cfg;
            _parentVm = parentVm;
            _authApi = authApi;

            _loginUsername = username;
            _loginPassword = password;

            this.WhenAnyValue(p => p.TimeoutSecondsLeft)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(ConfirmButtonText));
                    this.RaisePropertyChanged(nameof(ConfirmButtonEnabled));
                });
        }

        public override void Activated()
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
            // TODO: Remove Task.Delay here.
            await Task.Delay(1000);
            var resp = await _authApi.AuthenticateAsync(_loginUsername, _loginPassword);

            if (resp.IsSuccess)
            {
                var loginInfo = resp.LoginInfo;
                if (_cfg.Logins.Lookup(loginInfo.UserId).HasValue)
                {
                    // Already had a login like this??
                    // TODO: Immediately sign out the token here.
                    _cfg.SelectedLoginId = loginInfo.UserId;
                    _parentVm.SwitchToLogin();
                    return;
                }

                _cfg.AddLogin(loginInfo);
                _cfg.SelectedLoginId = loginInfo.UserId;
                _parentVm.SwitchToLogin();
            }
            else
            {
                // TODO: Display errors
            }
        }
    }
}
