using System;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Models;
using SS14.Launcher.Models.Logins;

namespace SS14.Launcher.ViewModels.Login
{
    public class LoginViewModel : BaseLoginViewModel, IErrorOverlayOwner
    {
        private readonly DataManager _cfg;
        public MainWindowLoginViewModel ParentVM { get; }
        private readonly AuthApi _authApi;
        private readonly LoginManager _loginMgr;

        [Reactive] public string EditingUsername { get; set; } = "";
        [Reactive] public string EditingPassword { get; set; } = "";

        [Reactive] public bool IsInputValid { get; private set; }

        public LoginViewModel(DataManager cfg, MainWindowLoginViewModel parentVm, AuthApi authApi, LoginManager loginMgr)
        {
            BusyText = "Logging in...";
            _authApi = authApi;
            _loginMgr = loginMgr;
            _cfg = cfg;
            ParentVM = parentVm;

            this.WhenAnyValue(x => x.EditingUsername, x => x.EditingPassword)
                .Subscribe(s => { IsInputValid = !string.IsNullOrEmpty(s.Item1) && !string.IsNullOrEmpty(s.Item2); });
        }

        public async void OnLogInButtonPressed()
        {
            if (!IsInputValid)
            {
                return;
            }

            Busy = true;
            try
            {
                var resp = await _authApi.AuthenticateAsync(EditingUsername, EditingPassword);

                if (resp.IsSuccess)
                {
                    var loginInfo = resp.LoginInfo;
                    if (_cfg.Logins.Lookup(loginInfo.UserId).HasValue)
                    {
                        // Already had this login, apparently.
                        // Thanks user.
                        // Log the token out since we don't need it.

                        await _authApi.LogoutTokenAsync(loginInfo.Token.Token);
                        _loginMgr.ActiveAccountId = loginInfo.UserId;
                        return;
                    }

                    _cfg.AddLogin(loginInfo);
                    _loginMgr.ActiveAccountId = loginInfo.UserId;
                }
                else
                {
                    OverlayControl = new AuthErrorsOverlayViewModel(this, "Unable to log in", resp.Errors);
                }
            }
            finally
            {
                Busy = false;
            }
        }

        public void OverlayOk()
        {
            OverlayControl = null;
        }
    }
}
