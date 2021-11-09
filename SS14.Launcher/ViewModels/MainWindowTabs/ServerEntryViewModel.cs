using System;
using Avalonia.Media;
using ReactiveUI;
using Splat;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.ServerStatus;

namespace SS14.Launcher.ViewModels.MainWindowTabs;

public sealed class ServerEntryViewModel : ViewModelBase
{
    private static readonly Color ColorAltBackground = Color.Parse("#262626");

    private readonly ServerStatusCache _cache;
    private readonly IServerStatusData _cacheData;
    private readonly DataManager _cfg;
    private readonly MainWindowViewModel _windowVm;
    private bool _isAltBackground;
    private string Address => _cacheData.Address;
    private string _fallbackName = string.Empty;

    public ServerEntryViewModel(MainWindowViewModel windowVm,string address)
    {
        _cache = Locator.Current.GetService<ServerStatusCache>();
        _cfg = Locator.Current.GetService<DataManager>();
        _windowVm = windowVm;
        _cacheData = _cache.GetStatusFor(address);

        this.WhenAnyValue(x => x.IsAltBackground)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(BackgroundColor)));

        this.WhenAnyValue(x => x._cacheData.PlayerCount)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(ServerStatusString)));

        this.WhenAnyValue(x => x._cacheData.Status)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(IsOnline));
                this.RaisePropertyChanged(nameof(ServerStatusString));
            });

        this.WhenAnyValue(x => x._cacheData.Name)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(Name)));

        this.WhenAnyValue(x => x._cacheData.Ping)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(PingText)));

        _cfg.FavoriteServers.Connect()
            .Subscribe(_ => { this.RaisePropertyChanged(nameof(FavoriteButtonText)); });
    }

    public ServerEntryViewModel(MainWindowViewModel windowVm, FavoriteServer favorite)
        : this(windowVm, favorite.Address)
    {
        Favorite = favorite;
    }

    public void DoInitialUpdate()
    {
        _cache.InitialUpdateStatus(_cacheData);
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


    // Avalonia can't currently do alternating backgrounds in ItemsControl easily.
    // So we have to implement them manually in the view model.
    public bool IsAltBackground
    {
        get => _isAltBackground;
        set => this.RaiseAndSetIfChanged(ref _isAltBackground, value);
    }

    public string FallbackName
    {
        get => _fallbackName;
        set
        {
            this.RaiseAndSetIfChanged(ref _fallbackName, value);
            this.RaisePropertyChanged(nameof(Name));
        }
    }

    public Color BackgroundColor => IsAltBackground ? ColorAltBackground : Colors.Transparent;

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
    }
}