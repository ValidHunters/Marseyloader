using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
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

    public ReadOnlyObservableCollection<ServerEntryViewModel> SearchedServers { get; }

    public SourceList<ServerEntryViewModel> AllServers { get; } = new();
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
        _statusCache = new ServerStatusCache();
        _http = Locator.Current.GetRequiredService<HttpClient>();

        var filter = this.WhenAnyValue(x => x.SearchString)
            .Select<string?, Func<ServerEntryViewModel, bool>>(s =>
            {
                if (string.IsNullOrWhiteSpace(s))
                    return _ => true;

                return server => server.Name.Contains(s, StringComparison.CurrentCultureIgnoreCase);
            });

        var resort = AllServers
            .Connect()
            .WhenPropertyChanged(p => p.CacheData.PlayerCount)
            .Select(_ => Unit.Default);

        AllServers
            .Connect()
            .Filter(filter)
            .FilterOnObservable(s => s.WhenAnyValue(sv => sv.IsOnline))
            .Sort(SortExpressionComparer<ServerEntryViewModel>.Descending(p => p.CacheData.PlayerCount), resort: resort)
            .Bind(out var searchedServers)
            .Subscribe(a =>
            {
                this.RaisePropertyChanged(nameof(ListEmptyText));
                this.RaisePropertyChanged(nameof(ListVisible));
            });

        SearchedServers = searchedServers;

        this.WhenAnyValue(x => x.Status)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(ListEmptyText));
                this.RaisePropertyChanged(nameof(ListVisible));
            });
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
        _statusCache.Clear();
        await RefreshServerList(_refreshCancel.Token);
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

            var entries = await response.Content.AsJson<ServerListEntry[]>();

            Status = RefreshListStatus.Updated;

            AllServers.AddRange(entries.Select(e =>
            {
                var cacheData = _statusCache.GetStatusFor(e.Address);
                _statusCache.InitialUpdateStatus(cacheData);

                return new ServerEntryViewModel(_windowVm, cacheData)
                {
                    FallbackName = e.Name
                };
            }));
        }
        catch (OperationCanceledException)
        {
            Status = RefreshListStatus.NotUpdated;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to fetch server list due to exception");
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
