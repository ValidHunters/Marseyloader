using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Models;
using SS14.Launcher.ViewModels.MainWindowTabs;
using SS14.Launcher.Views;

namespace SS14.Launcher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly DataManager _cfg;

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
            var loginManager = new LoginManager(cfg, authApi);

            ServersTab = new ServerListTabViewModel(cfg, statusCache, updater);
            NewsTab = new NewsTabViewModel();
            HomeTab = new HomePageViewModel(this, cfg, statusCache, updater);
            OptionsTab = new OptionsTabViewModel();

            Tabs = new MainWindowTabViewModel[]
            {
                HomeTab,
                ServersTab,
                NewsTab,
                OptionsTab
            };

            this.WhenAnyValue(x => x.SelectedIndex).Subscribe(i => Tabs[i].Selected());

            this.WhenAnyValue(x => x._cfg.SelectedLogin)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(Username));
                    this.RaisePropertyChanged(nameof(LoggedIn));
                    this.RaisePropertyChanged(nameof(LoginText));
                    this.RaisePropertyChanged(nameof(ManageAccountText));
                });

            _cfg.Logins.Connect()
                .Subscribe(_ => { this.RaisePropertyChanged(nameof(AccountDropDownVisible)); });

            AccountDropDown = new AccountDropDownViewModel(cfg, authApi);
            LoginViewModel = new MainWindowLoginViewModel(cfg, authApi);
        }

        public MainWindow? Control { get; set; }

        public IReadOnlyList<MainWindowTabViewModel> Tabs { get; }

        public bool LoggedIn => _cfg.SelectedLoginId != null;
        public string LoginText => LoggedIn ? $"'Logged in' as {Username}." : "Not logged in.";
        public string ManageAccountText => LoggedIn ? "Change Account..." : "Log in...";
        private string? Username => _cfg.SelectedLogin?.Username;
        public bool AccountDropDownVisible => _cfg.Logins.Count != 0;

        public AccountDropDownViewModel AccountDropDown { get; }

        public MainWindowLoginViewModel LoginViewModel { get; }

        [Reactive] public string? BusyTask { get; private set; }

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
        }

        private async Task CheckAccounts()
        {
            // Check if accounts are still valid and refresh their tokens if necessary.
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
            await Task.Delay(1000);
            var launcherVersionUri =
                new Uri($"{Updater.JenkinsBaseUrl}/userContent/current_launcher_version.txt");
            var versionRequest = await Global.GlobalHttpClient.GetAsync(launcherVersionUri);
            versionRequest.EnsureSuccessStatusCode();
            OutOfDate = ConfigConstants.CurrentLauncherVersion != (await versionRequest.Content.ReadAsStringAsync()).Trim();
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
    }
}
