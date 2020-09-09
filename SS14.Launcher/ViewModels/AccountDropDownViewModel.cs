using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using JetBrains.Annotations;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Models;
using SS14.Launcher.Views;

namespace SS14.Launcher.ViewModels
{
    public class AccountDropDownViewModel : ViewModelBase
    {
        private readonly DataManager _cfg;
        private readonly AuthApi _authApi;
        private readonly ReadOnlyObservableCollection<LoginInfo> _accounts;

        public ReadOnlyObservableCollection<LoginInfo> Accounts => _accounts;

        public AccountDropDownViewModel(DataManager cfg, AuthApi authApi)
        {
            _cfg = cfg;
            _authApi = authApi;

            this.WhenAnyValue(x => x._cfg.SelectedLogin)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(LoginText));
                    this.RaisePropertyChanged(nameof(AccountSwitchText));
                    this.RaisePropertyChanged(nameof(LogoutText));
                    this.RaisePropertyChanged(nameof(AccountControlsVisible));
                    this.RaisePropertyChanged(nameof(AccountSwitchVisible));
                });

            _cfg.Logins.Connect().Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(LogoutText));
                this.RaisePropertyChanged(nameof(AccountSwitchVisible));
            });

            var filterObservable = this.WhenAnyValue(x => x._cfg.SelectedLogin)
                .Select(MakeFilter);

            _cfg.Logins.Connect()
                .Filter(filterObservable)
                .Bind(out _accounts)
                .Subscribe();
        }

        private static Func<LoginInfo?, bool> MakeFilter(LoginInfo? selected)
        {
            return l => l != selected;
        }

        public string LoginText => _cfg.SelectedLogin?.Username ?? "No account selected";

        public AccountDropDown? Control { get; set; }

        public string LogoutText => _cfg.Logins.Count == 1 ? "Log out" : $"Log out of {_cfg.SelectedLogin?.Username}";

        public bool AccountSwitchVisible => _cfg.Logins.Count > 1 || _cfg.SelectedLogin == null;
        public string AccountSwitchText => _cfg.SelectedLogin != null ? "Switch account:" : "Select account:";
        public bool AccountControlsVisible => _cfg.SelectedLogin != null;

        [Reactive] public bool IsDropDownOpen { get; set; }

        public async void LogoutPressed()
        {
            IsDropDownOpen = false;

            if (_cfg.SelectedLogin != null)
            {
                await _authApi.LogoutTokenAsync(_cfg.SelectedLogin.Token);
                _cfg.RemoveLogin(_cfg.SelectedLogin);
            }
        }

        [UsedImplicitly]
        public void AccountButtonPressed(Guid id)
        {
            IsDropDownOpen = false;

            _cfg.SelectedLoginId = id;
        }

        public void AddAccountPressed()
        {
            IsDropDownOpen = false;

            _cfg.SelectedLoginId = null;
        }
    }
}
