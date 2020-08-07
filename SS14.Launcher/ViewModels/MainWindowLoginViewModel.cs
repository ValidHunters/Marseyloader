using System;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Models;
using SS14.Launcher.ViewModels.Login;

namespace SS14.Launcher.ViewModels
{
    public class MainWindowLoginViewModel : ViewModelBase
    {
        public LoginViewModel Login { get; }
        public RegisterViewModel Register { get; }

        [Reactive] public bool Registering { get; set; }

        public MainWindowLoginViewModel(ConfigurationManager cfg)
        {
            Login = new LoginViewModel(cfg, this);
            Register = new RegisterViewModel(cfg, this);
        }

        public string? Version => $"v{LauncherVersion.Version}";
    }
}