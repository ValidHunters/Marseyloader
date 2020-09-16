using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using SS14.Launcher.Models;
using SS14.Launcher.Models.Logins;
using SS14.Launcher.Models.ServerStatus;
using SS14.Launcher.Utility;
using SS14.Launcher.ViewModels.Login;
using SS14.Launcher.ViewModels.MainWindowTabs;
using SS14.Launcher.Views;

namespace SS14.Launcher.ViewModels
{
    public sealed class MainWindowViewModel : ViewModelBase, IErrorOverlayOwner
    {
        private readonly DataManager _cfg;
        private readonly LoginManager _loginMgr;

        private int _selectedIndex;

        [Reactive] public bool OutOfDate { get; private set; }

        public HomePageViewModel HomeTab { get; }
        public ServerListTabViewModel ServersTab { get; }
        public NewsTabViewModel NewsTab { get; }
        public OptionsTabViewModel OptionsTab { get; }


        public MainWindowViewModel(DataManager cfg, ServerStatusCache statusCache, Updater updater)
        {
            _cfg = cfg;
            var authApi = new AuthApi(cfg);
            _loginMgr = new LoginManager(cfg, authApi);

            ServersTab = new ServerListTabViewModel(cfg, statusCache, updater, _loginMgr);
            NewsTab = new NewsTabViewModel();
            HomeTab = new HomePageViewModel(this, cfg, statusCache, updater, _loginMgr);
            OptionsTab = new OptionsTabViewModel(cfg);

            Tabs = new MainWindowTabViewModel[]
            {
                HomeTab,
                ServersTab,
                NewsTab,
                OptionsTab
            };

            AccountDropDown = new AccountDropDownViewModel(this, cfg, authApi, _loginMgr);
            LoginViewModel = new MainWindowLoginViewModel(cfg, authApi, _loginMgr);

            this.WhenAnyValue(x => x.SelectedIndex).Subscribe(i => Tabs[i].Selected());

            this.WhenAnyValue(x => x._loginMgr.ActiveAccount)
                .Subscribe(s =>
                {
                    this.RaisePropertyChanged(nameof(Username));
                    this.RaisePropertyChanged(nameof(LoggedIn));
                    this.RaisePropertyChanged(nameof(LoginText));
                    this.RaisePropertyChanged(nameof(ManageAccountText));
                });

            _cfg.Logins.Connect()
                .Subscribe(_ => { this.RaisePropertyChanged(nameof(AccountDropDownVisible)); });

            // If we leave the login view model (by an account getting selected)
            // we reset it to login state
            this.WhenAnyValue(x => x.LoggedIn)
                .DistinctUntilChanged() // Only when change.
                .Where(p => !p)
                .Subscribe(x => LoginViewModel.SwitchToLogin());
        }

        public MainWindow? Control { get; set; }

        public IReadOnlyList<MainWindowTabViewModel> Tabs { get; }

        public bool LoggedIn => _loginMgr.ActiveAccount != null;
        public string LoginText => LoggedIn ? $"'Logged in' as {Username}." : "Not logged in.";
        public string ManageAccountText => LoggedIn ? "Change Account..." : "Log in...";
        private string? Username => _loginMgr.ActiveAccount?.Username;
        public bool AccountDropDownVisible => _loginMgr.Logins.Count != 0;

        public AccountDropDownViewModel AccountDropDown { get; }

        public MainWindowLoginViewModel LoginViewModel { get; }

        [Reactive] public string? BusyTask { get; private set; }
        [Reactive] public ViewModelBase? OverlayViewModel { get; private set; }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
        }

        public async void OnWindowInitialized()
        {
            BusyTask = "Checking for launcher update...";
            await CheckLauncherUpdate();
            BusyTask = "Refreshing login status...";
            await CheckAccounts();
            BusyTask = null;

            if (_cfg.SelectedLoginId is { } g && _loginMgr.Logins.TryLookup(g, out var login))
            {
                TrySwitchToAccount(login);
            }
        }

        private async Task CheckAccounts()
        {
            // Check if accounts are still valid and refresh their tokens if necessary.
            await _loginMgr.Initialize();
        }

        public void OnDiscordButtonPressed()
        {
            Helpers.OpenUri(new Uri(ConfigConstants.DiscordUrl));
        }

        public void OnWebsiteButtonPressed()
        {
            Helpers.OpenUri(new Uri(ConfigConstants.WebsiteUrl));
        }

        private async Task CheckLauncherUpdate()
        {
            // await Task.Delay(1000);
            var launcherVersionUri =
                new Uri($"{Updater.JenkinsBaseUrl}/userContent/current_launcher_version.txt");
            var versionRequest = await Global.GlobalHttpClient.GetAsync(launcherVersionUri);
            versionRequest.EnsureSuccessStatusCode();
            OutOfDate = ConfigConstants.CurrentLauncherVersion !=
                        (await versionRequest.Content.ReadAsStringAsync()).Trim();
        }

        public void ExitPressed()
        {
            Control?.Close();
        }

        public void DownloadPressed()
        {
            Helpers.OpenUri(new Uri(ConfigConstants.DownloadUrl));
        }

        public void SelectTabServers()
        {
            SelectedIndex = Tabs.IndexOf(ServersTab);
        }

        public void TrySwitchToAccount(LoggedInAccount account)
        {
            switch (account.Status)
            {
                case AccountLoginStatus.Unsure:
                    TrySelectUnsureAccount(account);
                    break;

                case AccountLoginStatus.Available:
                    _loginMgr.ActiveAccount = account;
                    break;

                case AccountLoginStatus.Expired:
                    _loginMgr.ActiveAccount = null;
                    LoginViewModel.SwitchToExpiredLogin(account);
                    break;
            }
        }

        private async void TrySelectUnsureAccount(LoggedInAccount account)
        {
            BusyTask = "Checking account status";
            try
            {
                await _loginMgr.UpdateSingleAccountStatus(account);

                // Can't be unsure, that'd have thrown.
                Debug.Assert(account.Status != AccountLoginStatus.Unsure);
                TrySwitchToAccount(account);
            }
            catch (AuthApiException e)
            {
                Log.Warning(e, "AuthApiException while trying to refresh account {login}", account.LoginInfo);
                OverlayViewModel = new AuthErrorsOverlayViewModel(this, "Error connecting to authentication server",
                    new[]
                    {
                        e.InnerException?.Message ?? "Unknown error occured"
                    });
            }
            finally
            {
                BusyTask = null;
            }
        }

        public void OverlayOk()
        {
            OverlayViewModel = null;
        }
    }
}
