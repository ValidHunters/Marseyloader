using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Splat;
using SS14.Launcher.Models.ServerStatus;
using SS14.Launcher.Utility;

namespace SS14.Launcher.ViewModels.MainWindowTabs;

public class ServerListTabViewModel : MainWindowTabViewModel
{
    private readonly ServerStatusCache _statusCache;
    private readonly MainWindowViewModel _windowVm;
    private readonly HttpClient _http;
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

    public ServerListTabViewModel(MainWindowViewModel windowVm)
    {
        _windowVm = windowVm;
        _statusCache = Locator.Current.GetRequiredService<ServerStatusCache>();
        _http = Locator.Current.GetRequiredService<HttpClient>();

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
                await _http.GetAsync(ConfigConstants.HubUrl + "api/servers", cancel);

            response.EnsureSuccessStatusCode();

            var entries = await response.Content.AsJson<List<ServerListEntry>>();

            Status = RefreshListStatus.Updated;

            AllServers.AddRange(entries.Select(e =>
                new ServerEntryViewModel(_windowVm, e.Address)
                {
                    FallbackName = e.Name
                }));
        }
        catch (OperationCanceledException)
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
