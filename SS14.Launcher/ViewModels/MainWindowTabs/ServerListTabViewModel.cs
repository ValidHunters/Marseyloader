using System;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using ReactiveUI;
using SS14.Launcher.Models;

namespace SS14.Launcher.ViewModels.MainWindowTabs
{
    public class ServerListTabViewModel : MainWindowTabViewModel
    {
        private readonly ServerStatusCache _statusCache;

        private static readonly (string name, string address)[] PresetServers =
        {
            ("Wizard's Den", "ss14s://builds.spacestation14.io/ss14_server")
        };

        private string? _searchString;

        public ServerListTabViewModel(ConfigurationManager cfg, ServerStatusCache statusCache, Updater updater)
        {
            _statusCache = statusCache;
            AllServers = new ObservableCollection<ServerEntryViewModel>(PresetServers.Select(a =>
            {
                var (name, address) = a;
                return new ServerEntryViewModel(statusCache, cfg, updater, address) {FallbackName = name};
            }));

            this.WhenAnyValue(x => x.SearchString)
                .Subscribe(v =>
                {
                    SearchedServers.Clear();
                    if (string.IsNullOrEmpty(v))
                    {
                        SearchedServers.AddRange(AllServers);
                    }
                    else
                    {
                        SearchedServers.AddRange(AllServers.Where(s =>
                            s.Name.Contains(v, StringComparison.CurrentCultureIgnoreCase)));
                    }

                    var alt = false;
                    foreach (var server in SearchedServers)
                    {
                        server.IsAltBackground = alt;
                        alt ^= true;
                    }
                });
        }

        public override void Selected()
        {
            foreach (var server in AllServers)
            {
                server.TabSelected();
            }
        }

        public override string Name => "Servers";

        public string? SearchString
        {
            get => _searchString;
            set => this.RaiseAndSetIfChanged(ref _searchString, value);
        }

        public ObservableCollection<ServerEntryViewModel> SearchedServers { get; }
            = new ObservableCollection<ServerEntryViewModel>();

        public ObservableCollection<ServerEntryViewModel> AllServers { get; }

        public void RefreshPressed()
        {
            _statusCache.Refresh();
        }
    }
}