using System;
using System.Diagnostics;
using System.Net.Mail;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Robust.Shared.AuthLib;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.Logins;

namespace SS14.Launcher.ViewModels.Login;

public class RegisterViewModel : BaseLoginViewModel, IErrorOverlayOwner
{
    private readonly DataManager _cfg;
    public MainWindowLoginViewModel ParentVM { get; }
    private readonly AuthApi _authApi;
    private readonly LoginManager _loginMgr;

    [Reactive] public string EditingUsername { get; set; } = "";
    [Reactive] public string EditingPassword { get; set; } = "";
    [Reactive] public string EditingPasswordConfirm { get; set; } = "";
    [Reactive] public string EditingEmail { get; set; } = "";

    [Reactive] public bool IsInputValid { get; private set; }
    [Reactive] public string InvalidReason { get; private set; } = " ";

    [Reactive] public bool Is13OrOlder { get; set; }


    public RegisterViewModel(DataManager cfg, MainWindowLoginViewModel parentVm, AuthApi authApi, LoginManager loginMgr)
    {
        _cfg = cfg;
        ParentVM = parentVm;
        _authApi = authApi;
        _loginMgr = loginMgr;

        this.WhenAnyValue(x => x.EditingUsername, x => x.EditingPassword, x => x.EditingPasswordConfirm,
                x => x.EditingEmail, x => x.Is13OrOlder)
            .Subscribe(UpdateInputValid);
    }

    private void UpdateInputValid((string user, string pass, string passConfirm, string email, bool is13OrOlder) s)
    {
        var (user, pass, passConfirm, email, is13OrOlder) = s;

        IsInputValid = false;
        if (!UsernameHelpers.IsNameValid(user, out var reason))
        {
            InvalidReason = reason switch
            {
                UsernameHelpers.UsernameInvalidReason.Empty => "Username is empty",
                UsernameHelpers.UsernameInvalidReason.TooLong => "Username is too long",
                UsernameHelpers.UsernameInvalidReason.TooShort => "Username is too short",
                UsernameHelpers.UsernameInvalidReason.InvalidCharacter => "Username contains an invalid character",
                _ => "???"
            };
            return;
        }

        if (string.IsNullOrEmpty(email))
        {
            InvalidReason = "Email is empty";
            return;
        }

        if (!MailAddress.TryCreate(email, out _))
        {
            InvalidReason = "Email is invalid";
            return;
        }

        if (string.IsNullOrEmpty(pass))
        {
            InvalidReason = "Password is empty";
            return;
        }

        if (pass != passConfirm)
        {
            InvalidReason = "Confirm password does not match";
            return;
        }

        if (!is13OrOlder)
        {
            InvalidReason = "You must be 13 or older";
            return;
        }

        InvalidReason = " ";
        IsInputValid = true;
    }

    public async void OnRegisterInButtonPressed()
    {
        if (!IsInputValid)
        {
            return;
        }

        BusyText = "Registering account...";
        Busy = true;
        try
        {
            var result = await _authApi.RegisterAsync(EditingUsername, EditingEmail, EditingPassword);
            if (result.IsSuccess)
            {
                var status = result.Status;
                if (status == RegisterResponseStatus.Registered)
                {
                    BusyText = "Logging in...";
                    // No confirmation needed, log in immediately.
                    var resp = await _authApi.AuthenticateAsync(EditingUsername, EditingPassword);

                    await LoginViewModel.DoLogin(this, resp, _loginMgr, _authApi);
                }
                else
                {
                    Debug.Assert(status == RegisterResponseStatus.RegisteredNeedConfirmation);

                    ParentVM.SwitchToRegisterNeedsConfirmation(EditingUsername, EditingPassword);
                }
            }
            else
            {
                OverlayControl = new AuthErrorsOverlayViewModel(this, "Unable to register", result.Errors);
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