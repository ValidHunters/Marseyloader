using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using SS14.Launcher.Models;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.EngineManager;
using SS14.Launcher.Models.Logins;
using SS14.Launcher.Models.ServerStatus;

namespace SS14.Launcher.ViewModels.MainWindowTabs
{
    public class ServerListTabViewModel : MainWindowTabViewModel
    {
        private readonly DataManager _cfg;
        private readonly ServerStatusCache _statusCache;
        private readonly Updater _updater;
        private readonly LoginManager _loginMgr;
        private readonly MainWindowViewModel _windowVm;
        private readonly IEngineManager _engineMgr;
        private CancellationTokenSource? _refreshCancel;

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

        public ServerListTabViewModel(DataManager cfg,
            ServerStatusCache statusCache,
            Updater updater,
            LoginManager loginMgr,
            MainWindowViewModel windowVm,
            IEngineManager engineMgr)
        {
            _cfg = cfg;
            _statusCache = statusCache;
            _updater = updater;
            _loginMgr = loginMgr;
            _windowVm = windowVm;
            _engineMgr = engineMgr;

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
                _refreshCancel?.Cancel();
                _refreshCancel = new CancellationTokenSource();
                await RefreshServerList(_refreshCancel.Token);
            }
        }

        public async void RefreshPressed()
        {
            _refreshCancel?.Cancel();
            _refreshCancel = new CancellationTokenSource();
            await RefreshServerList(_refreshCancel.Token);
            _statusCache.Refresh();
        }

        private async Task RefreshServerList(CancellationToken cancel)
        {
            AllServers.Clear();
            Status = RefreshListStatus.Updating;

            try
            {
                using var response =
                    await Global.GlobalHttpClient.GetAsync(ConfigConstants.HubUrl + "api/servers", cancel);

                response.EnsureSuccessStatusCode();

                // TODO: .NET 5 has an overload of ReadAsStringAsync with cancellation support.
                var respStr = await response.Content.ReadAsStringAsync();

                cancel.ThrowIfCancellationRequested();

                var entries = JsonConvert.DeserializeObject<List<ServerListEntry>>(respStr);

                Status = RefreshListStatus.Updated;

                AllServers.AddRange(entries.Select(e =>
                    new ServerEntryViewModel(_statusCache, _cfg, _updater, _loginMgr, _windowVm, _engineMgr, e.Address)
                    {
                        FallbackName = e.Name
                    }));
            }
            catch (TaskCanceledException)
            {
                Status = RefreshListStatus.NotUpdated;
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
