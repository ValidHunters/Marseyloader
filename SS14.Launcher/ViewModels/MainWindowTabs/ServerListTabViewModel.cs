using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using SS14.Launcher.Models;

namespace SS14.Launcher.ViewModels.MainWindowTabs
{
    public class ServerListTabViewModel : MainWindowTabViewModel
    {
        private readonly ConfigurationManager _cfg;
        private readonly ServerStatusCache _statusCache;
        private readonly Updater _updater;

        public ObservableCollection<ServerEntryViewModel> SearchedServers { get; }
            = new ObservableCollection<ServerEntryViewModel>();

        public ObservableCollection<ServerEntryViewModel> AllServers { get; }
            = new ObservableCollection<ServerEntryViewModel>();

        [Reactive] private RefreshListStatus Status { get; set; } = RefreshListStatus.NotUpdated;

        public override string Name => "Servers";

        [Reactive] public string? SearchString { get; set; }

        public bool ListVisible => Status == RefreshListStatus.Updated && SearchedServers.Count != 0;

        public string ListEmptyText
        {
            get
            {
                if (Status == RefreshListStatus.Error)
                {
                    return "There was an error fetching the master server list.";
                }

                if (Status == RefreshListStatus.Updating)
                {
                    return "Updating server list...";
                }

                if (AllServers.Count != 0)
                {
                    return "There's no public servers, apparently?";
                }

                if (SearchedServers.Count != 0)
                {
                    return "No servers match your search.";
                }

                return "";
            }
        }

        public ServerListTabViewModel(ConfigurationManager cfg, ServerStatusCache statusCache, Updater updater)
        {
            _cfg = cfg;
            _statusCache = statusCache;
            _updater = updater;

            AllServers.CollectionChanged += (s, e) =>
            {
                foreach (var server in AllServers)
                {
                    server.DoInitialUpdate();
                }

                RepopulateServerList();
            };

            this.WhenAnyValue(x => x.SearchString)
                .Subscribe(_ => RepopulateServerList());

            this.WhenAnyValue(x => x.Status)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(ListEmptyText));
                    this.RaisePropertyChanged(nameof(ListVisible));
                });

            SearchedServers.CollectionChanged += (s, e) =>
            {
                this.RaisePropertyChanged(nameof(ListEmptyText));
                this.RaisePropertyChanged(nameof(ListVisible));
            };
        }

        private void RepopulateServerList()
        {
            SearchedServers.Clear();
            if (string.IsNullOrEmpty(SearchString))
            {
                SearchedServers.AddRange(AllServers);
            }
            else
            {
                SearchedServers.AddRange(AllServers.Where(s =>
                    s.Name.Contains(SearchString, StringComparison.CurrentCultureIgnoreCase)));
            }

            var alt = false;
            foreach (var server in SearchedServers)
            {
                server.IsAltBackground = alt;
                alt ^= true;
            }
        }

        public override async void Selected()
        {
            if (Status == RefreshListStatus.NotUpdated)
            {
                await RefreshServerList();
            }
        }

        public async void RefreshPressed()
        {
            await RefreshServerList();
            _statusCache.Refresh();
        }

        private async Task RefreshServerList()
        {
            AllServers.Clear();
            Status = RefreshListStatus.Updating;

            try
            {
                var response = await Global.GlobalHttpClient.GetStringAsync(UrlConstants.HubUrl + "api/servers");

                var entries = JsonConvert.DeserializeObject<List<ServerListEntry>>(response);

                Status = RefreshListStatus.Updated;

                AllServers.AddRange(entries.Select(e =>
                    new ServerEntryViewModel(_statusCache, _cfg, _updater, e.Address)
                    {
                        FallbackName = e.Name
                    }));
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to fetch server list due to exception.");
                Status = RefreshListStatus.Error;
            }
        }

        private sealed class ServerListEntry
        {
            public string Address { get; set; } = default!;
            public string Name { get; set; } = default!;
        }

        private enum RefreshListStatus
        {
            NotUpdated,
            Updating,
            Updated,
            Error
        }
    }
}
