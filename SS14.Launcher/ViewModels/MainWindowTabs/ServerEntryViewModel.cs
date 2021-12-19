using System;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Splat;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.ServerStatus;
using SS14.Launcher.Utility;

namespace SS14.Launcher.ViewModels.MainWindowTabs;

public sealed class ServerEntryViewModel : ObservableRecipient, IRecipient<FavoritesChanged>, IViewModelBase
{
    private readonly IServerStatusData _cacheData;
    private readonly DataManager _cfg;
    private readonly MainWindowViewModel _windowVm;
    private string Address => _cacheData.Address;
    private string _fallbackName = string.Empty;

    public ServerEntryViewModel(MainWindowViewModel windowVm, IServerStatusData cacheData)
    {
        _cfg = Locator.Current.GetRequiredService<DataManager>();
        _windowVm = windowVm;
        _cacheData = cacheData;

        _cacheData.PropertyChanged += (_, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(IServerStatusData.PlayerCount):
                    OnPropertyChanged(nameof(ServerStatusString));
                    break;

                case nameof(IServerStatusData.Status):
                    OnPropertyChanged(nameof(IsOnline));
                    OnPropertyChanged(nameof(ServerStatusString));
                    break;

                case nameof(IServerStatusData.Name):
                    OnPropertyChanged(nameof(Name));
                    break;

                case nameof(IServerStatusData.Ping):
                    OnPropertyChanged(nameof(PingText));
                    break;
            }
        };
    }

    public ServerEntryViewModel(MainWindowViewModel windowVm, IServerStatusData cacheData, FavoriteServer favorite)
        : this(windowVm, cacheData)
    {
        Favorite = favorite;
    }

    public void ConnectPressed()
    {
        ConnectingViewModel.StartConnect(_windowVm, Address);
    }

    public FavoriteServer? Favorite { get; }

    public string Name => Favorite?.Name ?? _cacheData.Name ?? _fallbackName;
    public string FavoriteButtonText => IsFavorite ? "Remove Favorite" : "Add Favorite";
    private bool IsFavorite => _cfg.FavoriteServers.Lookup(Address).HasValue;

    public string ServerStatusString => _cacheData.Status switch
    {
        ServerStatusCode.Offline => "Unable to connect",
        ServerStatusCode.Online => _cacheData.PlayerCount == 1
            ? $"Online: {_cacheData.PlayerCount} player"
            : $"Online: {_cacheData.PlayerCount} players",
        ServerStatusCode.FetchingStatus => "Fetching status...",
        _ => throw new NotSupportedException()
    };

    public string PingText => $"Ping: {PingMs} ms";
    private int PingMs => (int) (_cacheData.Ping?.TotalMilliseconds ?? default);
    public bool IsOnline => _cacheData.Status == ServerStatusCode.Online;

    public string FallbackName
    {
        get => _fallbackName;
        set
        {
            SetProperty(ref _fallbackName, value);
            OnPropertyChanged(nameof(Name));
        }
    }

    public IServerStatusData CacheData => _cacheData;

    public void FavoriteButtonPressed()
    {
        if (IsFavorite)
        {
            // Remove favorite.
            _cfg.RemoveFavoriteServer(_cfg.FavoriteServers.Lookup(Address).Value);
        }
        else
        {
            var fav = new FavoriteServer(_cacheData.Name ?? FallbackName, Address);
            _cfg.AddFavoriteServer(fav);
        }

        _cfg.CommitConfig();
    }

    public void Receive(FavoritesChanged message)
    {
        OnPropertyChanged(nameof(FavoriteButtonText));
    }
}
