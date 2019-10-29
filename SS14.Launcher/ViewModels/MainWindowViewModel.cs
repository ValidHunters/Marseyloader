using System;
using System.Collections.Generic;
using ReactiveUI;

namespace SS14.Launcher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private int _selectedIndex;

        private readonly ServerListTabViewModel _servers;
        private readonly NewsTabViewModel _news;
        private readonly OptionsTabViewModel _options;
        private readonly HomePageViewModel _home;

        public MainWindowViewModel()
        {
            _servers = new ServerListTabViewModel();
            _news = new NewsTabViewModel();
            _options = new OptionsTabViewModel();
            _home = new HomePageViewModel();

            Tabs = new MainWindowTabViewModel[]
            {
                _home,
                _servers,
                _news,
                _options
            };

            this.WhenAnyValue(x => x.SelectedIndex).Subscribe(i => Tabs[i].Selected());
        }

        public IReadOnlyList<MainWindowTabViewModel> Tabs { get; }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
        }

        public void OnWindowInitialized()
        {
            _servers.Updater.OnWindowInitialized();
        }

        public void OnDiscordButtonPressed()
        {
            Helpers.OpenUri(new Uri("https://discord.gg/t2jac3p"));
        }

        public void OnWebsiteButtonPressed()
        {
            Helpers.OpenUri(new Uri("https://spacestation14.io"));
        }
    }
}