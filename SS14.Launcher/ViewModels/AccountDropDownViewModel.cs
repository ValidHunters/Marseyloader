using System;
using Avalonia.Controls;
using Avalonia.VisualTree;
using ReactiveUI;
using SS14.Launcher.Models;
using SS14.Launcher.Views;

namespace SS14.Launcher.ViewModels
{
    public class AccountDropDownViewModel : ViewModelBase
    {
        private readonly ConfigurationManager _cfg;

        public AccountDropDownViewModel(ConfigurationManager cfg)
        {
            _cfg = cfg;

            this.WhenAnyValue(x => x._cfg.UserName)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(Username));
                    this.RaisePropertyChanged(nameof(LoginText));
                    this.RaisePropertyChanged(nameof(ManageAccountText));
                });
        }

        public string LoginText => _cfg.UserName ?? "Not logged in";
        public string? Username => _cfg.UserName;
        public string ManageAccountText => _cfg.UserName != null ? "Change Account..." : "Log in...";

        public AccountDropDown? Control { get; set; }


        public async void ManageAccountPressed()
        {
            var res = await new LoginDialog {DefaultName = Username}.ShowDialog<string>((Window) Control.GetVisualRoot());
            if (res != null)
            {
                _cfg.UserName = res;
            }
        }


    }
}