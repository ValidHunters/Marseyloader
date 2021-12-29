using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.Logins;

namespace SS14.Launcher.ViewModels.Login;

public class ExpiredLoginViewModel : BaseLoginViewModel, IErrorOverlayOwner
{
    private readonly DataManager _cfg;
    private readonly MainWindowLoginViewModel _parentVm;
    private readonly AuthApi _authApi;
    private readonly LoginManager _loginMgr;

    public ExpiredLoginViewModel(
        DataManager cfg,
        MainWindowLoginViewModel parentVm,
        AuthApi authApi,
        LoginManager loginMgr,
        LoggedInAccount account)
    {
        _cfg = cfg;
        _parentVm = parentVm;
        _authApi = authApi;
        _loginMgr = loginMgr;
        Account = account;
    }

    [Reactive] public string EditingPassword { get; set; } = "";
    public LoggedInAccount Account { get; }

    public void OverlayOk()
    {
        OverlayControl = null;
    }

    public async void OnLogInButtonPressed()
    {
        Busy = true;
        try
        {
            var resp = await _authApi.AuthenticateAsync(Account.UserId, EditingPassword);

            await LoginViewModel.DoLogin(this, resp, _loginMgr, _authApi);

            _cfg.CommitConfig();
        }
        finally
        {
            Busy = false;
        }
    }

    public void OnLogOutButtonPressed()
    {
        _cfg.RemoveLogin(Account.LoginInfo);
        _cfg.CommitConfig();

        _parentVm.SwitchToLogin();
    }
}
