using System;
using System.Diagnostics;
using System.Net.Mail;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Robust.Shared.AuthLib;
using SS14.Launcher.Models;

namespace SS14.Launcher.ViewModels.Login
{
    public class RegisterViewModel : BaseLoginViewModel, IErrorOverlayOwner
    {
        private readonly ConfigurationManager _cfg;
        public MainWindowLoginViewModel ParentVM { get; }
        private readonly AuthApi _authApi;

        [Reactive] public string EditingUsername { get; set; } = "";
        [Reactive] public string EditingPassword { get; set; } = "";
        [Reactive] public string EditingPasswordConfirm { get; set; } = "";
        [Reactive] public string EditingEmail { get; set; } = "";

        [Reactive] public bool IsInputValid { get; private set; }
        [Reactive] public string InvalidReason { get; private set; } = " ";


        public RegisterViewModel(ConfigurationManager cfg, MainWindowLoginViewModel parentVm, AuthApi authApi)
        {
            _cfg = cfg;
            ParentVM = parentVm;
            _authApi = authApi;

            this.WhenAnyValue(x => x.EditingUsername, x => x.EditingPassword, x => x.EditingPasswordConfirm,
                    x => x.EditingEmail)
                .Subscribe(UpdateInputValid);
        }

        private void UpdateInputValid((string user, string pass, string passConfirm, string email) s)
        {
            var (user, pass, passConfirm, email) = s;

            IsInputValid = false;
            if (!UsernameHelpers.IsNameValid(user, out var reason))
            {
                InvalidReason = reason switch
                {
                    UsernameHelpers.UsernameInvalidReason.Empty => "Username is empty",
                    UsernameHelpers.UsernameInvalidReason.TooLong => "Username is too long",
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

            try
            {
                // TODO: .NET 5 has a Try* version of this, switch to that when .NET 5 is available.
                var unused = new MailAddress(email);
            }
            catch (FormatException)
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

            InvalidReason = " ";
            IsInputValid = true;
        }

        public async void OnRegisterInButtonPressed()
        {
            if (!IsInputValid)
            {
                return;
            }

            Busy = true;
            try
            {
                await Task.Delay(1000);
                var result = await _authApi.RegisterAsync(EditingUsername, EditingEmail, EditingPassword);
                if (result.IsSuccess)
                {
                    var status = result.Status;
                    if (status == RegisterResponseStatus.Registered)
                    {
                        // No confirmation needed, log in immediately.
                        var resp = await _authApi.AuthenticateAsync(EditingUsername, EditingPassword);

                        if (resp.IsSuccess)
                        {
                            var loginInfo = resp.LoginInfo;
                            if (_cfg.Logins.Lookup(loginInfo.UserId).HasValue)
                            {
                                throw new InvalidOperationException(
                                    "We just registered this account but also already had it??");
                            }

                            _cfg.AddLogin(loginInfo);
                            _cfg.SelectedLoginId = loginInfo.UserId;
                        }
                        else
                        {

                            // TODO: Display errors
                        }
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
}
