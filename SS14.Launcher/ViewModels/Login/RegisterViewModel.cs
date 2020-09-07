using System;
using System.Net.Mail;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Robust.Shared.AuthLib;
using SS14.Launcher.Models;

namespace SS14.Launcher.ViewModels.Login
{
    public class RegisterViewModel : ViewModelBase
    {
        private readonly ConfigurationManager _cfg;
        private readonly MainWindowLoginViewModel _parentVm;

        [Reactive] public string EditingUsername { get; set; } = "";
        [Reactive] public string EditingPassword { get; set; } = "";
        [Reactive] public string EditingPasswordConfirm { get; set; } = "";
        [Reactive] public string EditingEmail { get; set; } = "";

        [Reactive] public bool IsInputValid { get; private set; }
        [Reactive] public string InvalidReason { get; private set; } = " ";


        public RegisterViewModel(ConfigurationManager cfg, MainWindowLoginViewModel parentVm)
        {
            _cfg = cfg;
            _parentVm = parentVm;

            this.WhenAnyValue(x => x.EditingUsername, x => x.EditingPassword, x => x.EditingPasswordConfirm,
                    x => x.EditingEmail)
                .Subscribe(s =>
                {
                    var (user, pass, passConfirm, email) = s;

                    IsInputValid = false;
                    if (!UsernameHelpers.IsNameValid(user, out var reason))
                    {
                        InvalidReason = reason switch
                        {
                            UsernameHelpers.UsernameInvalidReason.Empty => "Username is empty",
                            UsernameHelpers.UsernameInvalidReason.TooLong => "Username is too long",
                            UsernameHelpers.UsernameInvalidReason.InvalidCharacter =>
                            "Username contains an invalid character",
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
                        var mailAddr = new MailAddress(email);
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
                });
        }

        public async void OnRegisterInButtonPressed()
        {
            if (!IsInputValid)
            {
                return;
            }


        }

        public void OnLoginButtonPressed()
        {
            EditingEmail = "";
            EditingUsername = "";
            EditingPassword = "";
            EditingPasswordConfirm = "";

            _parentVm.Screen = LoginScreen.Login;
        }
    }
}
