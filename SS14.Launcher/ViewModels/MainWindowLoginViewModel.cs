using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Models;

namespace SS14.Launcher.ViewModels
{
    public class MainWindowLoginViewModel : ViewModelBase
    {
        private readonly ConfigurationManager _cfg;

        [Reactive] public string EditingUsername { get; set; } = "";

        public MainWindowLoginViewModel(ConfigurationManager cfg)
        {
            _cfg = cfg;

            this.WhenAnyValue(x => x.EditingUsername)
                .Subscribe(s =>
                {
                    IsUsernameValid = UsernameHelpers.IsNameValid(s, out var reason);

                    InvalidReason = reason ?? " ";
                });
        }

        public string? Version
        {
            get
            {
                var version = typeof(MainWindowViewModel).Assembly.GetName().Version;
                return $"v{version}";
            }
        }

        [Reactive] public bool IsUsernameValid { get; private set; }
        [Reactive] public string InvalidReason { get; private set; } = " ";

        public void OnLogInButtonPressed()
        {
            if (!IsUsernameValid)
            {
                return;
            }

            _cfg.UserName = EditingUsername;
            EditingUsername = "";
        }
    }
}