using System;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Models.Logins;

namespace SS14.Launcher.ViewModels.Login
{
    public class LoginViewModel : BaseLoginViewModel, IErrorOverlayOwner
    {
        public MainWindowLoginViewModel ParentVM { get; }
        private readonly AuthApi _authApi;
        private readonly LoginManager _loginMgr;

        [Reactive] public string EditingUsername { get; set; } = "";
        [Reactive] public string EditingPassword { get; set; } = "";

        [Reactive] public bool IsInputValid { get; private set; }

        public LoginViewModel(MainWindowLoginViewModel parentVm, AuthApi authApi,
            LoginManager loginMgr)
        {
            BusyText = "Logging in...";
            _authApi = authApi;
            _loginMgr = loginMgr;
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

                await DoLogin(this, resp, _loginMgr, _authApi);
            }
            finally
            {
                Busy = false;
            }
        }

        public static async Task<bool> DoLogin<T>(
            T vm,
            AuthenticateResult resp,
            LoginManager loginMgr,
            AuthApi authApi)
            where T : BaseLoginViewModel, IErrorOverlayOwner
        {
            if (resp.IsSuccess)
            {
                var loginInfo = resp.LoginInfo;
                var oldLogin = loginMgr.Logins.Lookup(loginInfo.UserId);
                if (oldLogin.HasValue)
                {
                    // Already had this login, apparently.
                    // Thanks user.
                    //
                    // Log the OLD token out since we don't need two of them.
                    // This also has the upside of re-available-ing the account
                    // if the user used the main login prompt on an account we already had, but as expired.

                    await authApi.LogoutTokenAsync(oldLogin.Value.LoginInfo.Token.Token);
                    loginMgr.ActiveAccountId = loginInfo.UserId;
                    loginMgr.UpdateToNewToken(loginMgr.ActiveAccount!, loginInfo.Token);
                    return true;
                }

                loginMgr.AddFreshLogin(loginInfo);
                loginMgr.ActiveAccountId = loginInfo.UserId;
                return true;
            }

            vm.OverlayControl = new AuthErrorsOverlayViewModel(vm, "Unable to log in", resp.Errors);
            return false;
        }

        public void OverlayOk()
        {
            OverlayControl = null;
        }
    }
}
