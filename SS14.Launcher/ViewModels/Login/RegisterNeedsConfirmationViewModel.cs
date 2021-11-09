using System;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Models.Logins;

namespace SS14.Launcher.ViewModels.Login;

public class RegisterNeedsConfirmationViewModel : BaseLoginViewModel, IErrorOverlayOwner
{
    private const int TimeoutSeconds = 5;

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

    public RegisterNeedsConfirmationViewModel(MainWindowLoginViewModel parentVm,
        AuthApi authApi, string username, string password, LoginManager loginMgr)
    {
        BusyText = "Logging in...";
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

        await LoginViewModel.DoLogin(this, resp, _loginMgr, _authApi);

        Busy = false;
    }

    public void OverlayOk()
    {
        OverlayControl = null;
    }
}