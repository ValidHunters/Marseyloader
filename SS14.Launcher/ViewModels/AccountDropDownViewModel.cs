using System;
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

            this.WhenAnyValue(x => x._cfg.CurrentLogin)
                .Subscribe(_ => { this.RaisePropertyChanged(nameof(LoginText)); });
        }

        public string LoginText => _cfg.CurrentLogin?.Username ?? "Not logged in";

        public AccountDropDown? Control { get; set; }

        public void ManageAccountPressed()
        {
            _cfg.CurrentLogin = null;
            Control?.Popup.Close();
        }
    }
}