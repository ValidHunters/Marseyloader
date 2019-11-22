using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUI;
using SS14.Launcher.Models;
using SS14.Launcher.ViewModels.MainWindowTabs;
using SS14.Launcher.Views;

namespace SS14.Launcher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ConfigurationManager _cfg;
        private readonly Updater _updater;
        private const string CurrentLauncherVersion = "1";

        private int _selectedIndex;

        public MainWindowViewModel(ConfigurationManager cfg, ServerStatusCache statusCache, Updater updater)
        {
            _cfg = cfg;
            _updater = updater;

            var servers = new ServerListTabViewModel(cfg, statusCache, updater);
            var news = new NewsTabViewModel();
            var home = new HomePageViewModel(cfg, statusCache, updater);

            Tabs = new MainWindowTabViewModel[]
            {
                home,
                servers,
                news
            };

            this.WhenAnyValue(x => x.SelectedIndex).Subscribe(i => Tabs[i].Selected());

            this.WhenAnyValue(x => x._cfg.UserName)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(Username));
                    this.RaisePropertyChanged(nameof(LoggedIn));
                    this.RaisePropertyChanged(nameof(LoginText));
                    this.RaisePropertyChanged(nameof(ManageAccountText));
                });

            AccountDropDown = new AccountDropDownViewModel(cfg);
        }

        public MainWindow? Control { get; set; }

        public IReadOnlyList<MainWindowTabViewModel> Tabs { get; }

        private bool LoggedIn => _cfg.UserName != null;
        public string LoginText => LoggedIn ? $"'Logged in' as {Username}." : "Not logged in.";
        public string ManageAccountText => LoggedIn ? "Change Account..." : "Log in...";
        private string? Username => _cfg.UserName;

        public AccountDropDownViewModel AccountDropDown { get; }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
        }

        public async void OnWindowInitialized()
        {
            await CheckLauncherUpdate();
        }

        public void OnDiscordButtonPressed()
        {
            Helpers.OpenUri(new Uri("https://discord.gg/t2jac3p"));
        }

        public void OnWebsiteButtonPressed()
        {
            Helpers.OpenUri(new Uri("https://spacestation14.io"));
        }

        private async Task<bool> CheckLauncherUpdate()
        {
            var launcherVersionUri =
                new Uri($"{Global.JenkinsBaseUrl}/userContent/current_launcher_version.txt");
            var versionRequest = await Global.GlobalHttpClient.GetAsync(launcherVersionUri);
            versionRequest.EnsureSuccessStatusCode();
            var outOfDate = CurrentLauncherVersion != (await versionRequest.Content.ReadAsStringAsync()).Trim();

            if (!outOfDate)
            {
                return true;
            }

            async void ShowDialog()
            {
                // I seriously had an issue where the dialog opened too quickly so it didn't get placed correctly.
                await Task.Delay(100);

                await new OutOfDateDialog().ShowDialog(Control);

                Control?.Close();
            }

            ShowDialog();

            return false;
        }
    }
}