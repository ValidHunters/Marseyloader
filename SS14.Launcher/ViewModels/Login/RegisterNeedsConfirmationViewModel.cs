using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Models;
using SS14.Launcher.Models.Logins;

namespace SS14.Launcher.ViewModels.Login
{
    public class RegisterNeedsConfirmationViewModel : BaseLoginViewModel, IErrorOverlayOwner
    {
        private const int TimeoutSeconds = 5;

        private readonly DataManager _cfg;
        public MainWindowLoginViewModel ParentVM { get; }
        private readonly AuthApi _authApi;

        private readonly string _loginUsername;
        private readonly string _loginPassword;
        private readonly LoginManager _loginMgr;

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

        public RegisterNeedsConfirmationViewModel(DataManager cfg, MainWindowLoginViewModel parentVm,
            AuthApi authApi, string username, string password, LoginManager loginMgr)
        {
            BusyText = "Logging in...";
            _cfg = cfg;
            ParentVM = parentVm;
            _authApi = authApi;

            _loginUsername = username;
            _loginPassword = password;
            _loginMgr = loginMgr;

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
            Busy = true;
            var resp = await _authApi.AuthenticateAsync(_loginUsername, _loginPassword);

            if (resp.IsSuccess)
            {
                var loginInfo = resp.LoginInfo;
                if (_cfg.Logins.Lookup(loginInfo.UserId).HasValue)
                {
                    // Already had a login like this??
                    await _authApi.LogoutTokenAsync(loginInfo.Token.Token);

                    _loginMgr.ActiveAccountId = loginInfo.UserId;
                    ParentVM.SwitchToLogin();
                    return;
                }

                _cfg.AddLogin(loginInfo);
                _loginMgr.ActiveAccountId = loginInfo.UserId;
                ParentVM.SwitchToLogin();
            }
            else
            {
                Busy = false;
                OverlayControl = new AuthErrorsOverlayViewModel(this, "Unable to log in", resp.Errors);
            }
        }

        public void OverlayOk()
        {
            OverlayControl = null;
        }
    }
}
