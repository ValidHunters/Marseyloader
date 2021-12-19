using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Splat;
using SS14.Launcher.Models.ServerStatus;
using SS14.Launcher.Utility;

namespace SS14.Launcher.ViewModels.MainWindowTabs;

public class ServerListTabViewModel : MainWindowTabViewModel
{
    private readonly MainWindowViewModel _windowVm;
    private readonly HttpClient _http;
    private CancellationTokenSource? _refreshCancel;

    public ObservableCollection<ServerEntryViewModel> SearchedServers { get; } = new();

    private readonly List<ServerEntryViewModel> _allServers = new();
    private RefreshListStatus _status = RefreshListStatus.NotUpdated;
    private string? _searchString;

    private RefreshListStatus Status
    {
        get => _status;
        set
        {
            this.RaiseAndSetIfChanged(ref _status, value);
            this.RaisePropertyChanged(nameof(ListText));
            this.RaisePropertyChanged(nameof(ListTextVisible));
            this.RaisePropertyChanged(nameof(SpinnerVisible));
        }
    }

    public override string Name => "Servers";

    [Reactive]
    public string? SearchString
    {
        get => _searchString;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchString, value);
            UpdateSearchedList();
        }
    }

    public bool ListTextVisible => Status != RefreshListStatus.Updated;
    public bool SpinnerVisible => Status < RefreshListStatus.Updated;

    public string ListText
    {
        get
        {
            if (Status == RefreshListStatus.Error)
                return "There was an error fetching the master server list.";

            if (Status == RefreshListStatus.UpdatingMaster)
                return "Fetching master server list...";

            if (SearchedServers.Count == 0 && _allServers.Count != 0)
                return "No servers match your search.";

            if (Status == RefreshListStatus.Updating)
                return "Discovering servers...";

            if (Status == RefreshListStatus.NotUpdated)
                return "";

            if (_allServers.Count == 0)
                return "There's no public servers, apparently?";

            return "";
        }
    }

    public ServerListTabViewModel(MainWindowViewModel windowVm)
    {
        _windowVm = windowVm;
        _http = Locator.Current.GetRequiredService<HttpClient>();
    }

    public override void Selected()
    {
        if (Status == RefreshListStatus.NotUpdated)
        {
            _refreshCancel?.Cancel();
            _refreshCancel = new CancellationTokenSource();
            RefreshServerList(_refreshCancel.Token);
        }
    }

    public void RefreshPressed()
    {
        _refreshCancel?.Cancel();
        _allServers.Clear();
        SearchedServers.Clear();
        _refreshCancel = new CancellationTokenSource();
        RefreshServerList(_refreshCancel.Token);
    }

    private async void RefreshServerList(CancellationToken cancel)
    {
        _allServers.Clear();
        Status = RefreshListStatus.UpdatingMaster;

        try
        {
            using var response = await _http.GetAsync(ConfigConstants.HubUrl + "api/servers", cancel);

            response.EnsureSuccessStatusCode();

            var entries = await response.Content.AsJson<ServerListEntry[]>();

            Status = RefreshListStatus.Updating;

            await Parallel.ForEachAsync(entries, new ParallelOptions
            {
                MaxDegreeOfParallelism = 20,
                CancellationToken = cancel
            }, async (entry, token) =>
            {
                var status = new ServerStatusData(entry.Address);
                await ServerStatusCache.UpdateStatusFor(status, _http, token);

                if (status.Status == ServerStatusCode.Offline)
                    return;

                Dispatcher.UIThread.Post(() =>
                {
                    if (!cancel.IsCancellationRequested)
                    {
                        // Log.Information("{Name}: {Address}", status.Name, status.Address);
                        _allServers.Add(new ServerEntryViewModel(_windowVm, status) { FallbackName = entry.Name ?? "" });
                        UpdateSearchedList();
                    }
                });
            });

            Status = RefreshListStatus.Updated;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to fetch server list due to exception");
            Status = RefreshListStatus.Error;
        }
    }

    private void UpdateSearchedList()
    {
        var sortList = new List<ServerEntryViewModel>();

        foreach (var server in _allServers)
        {
            if (DoesSearchMatch(server))
                sortList.Add(server);
        }

        sortList.Sort(Comparer<ServerEntryViewModel>.Create((a, b) =>
            b.CacheData.PlayerCount.CompareTo(a.CacheData.PlayerCount)));

        SearchedServers.Clear();
        foreach (var server in sortList)
        {
            SearchedServers.Add(server);
        }
    }

    private bool DoesSearchMatch(ServerEntryViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(SearchString))
            return true;

        return vm.CacheData.Name != null &&
               vm.CacheData.Name.Contains(SearchString, StringComparison.CurrentCultureIgnoreCase);
    }

    private sealed class ServerListEntry
    {
        public string Address { get; set; } = default!;
        public string Name { get; set; } = default!;
    }

    private enum RefreshListStatus
    {
        /// <summary>
        /// Hasn't started updating yet?
        /// </summary>
        NotUpdated,

        /// <summary>
        /// Fetching master server list.
        /// </summary>
        UpdatingMaster,

        /// <summary>
        /// Fetched master server list and currently fetching information from master servers.
        /// </summary>
        Updating,

        /// <summary>
        /// Fetched information from ALL servers from the hub.
        /// </summary>
        Updated,

        /// <summary>
        /// An error occured.
        /// </summary>
        Error
    }
}
