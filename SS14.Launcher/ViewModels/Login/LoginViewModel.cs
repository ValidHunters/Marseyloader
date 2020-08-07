using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Models;

namespace SS14.Launcher.ViewModels.Login
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly ConfigurationManager _cfg;
        private readonly MainWindowLoginViewModel _parentVm;
        private readonly AuthApi _authApi;

        [Reactive] public string EditingUsername { get; set; } = "";
        [Reactive] public string EditingPassword { get; set; } = "";

        [Reactive] public bool IsInputValid { get; private set; }

        public LoginViewModel(ConfigurationManager cfg, MainWindowLoginViewModel parentVm)
        {
            _cfg = cfg;
            _parentVm = parentVm;

            this.WhenAnyValue(x => x.EditingUsername, x => x.EditingPassword)
                .Subscribe(s =>
                {
                    IsInputValid = !string.IsNullOrEmpty(s.Item1) && !string.IsNullOrEmpty(s.Item2);
                });


            _authApi = new AuthApi(cfg);
        }

        public async void OnLogInButtonPressed()
        {
            if (!IsInputValid)
            {
                return;
            }

            var resp = (LoginInfo) await _authApi.AuthenticateAsync(EditingUsername, EditingPassword);

            _cfg.CurrentLogin = resp;
        }

        public void OnRegisterButtonPressed()
        {
            _parentVm.Registering = true;
        }
    }
}