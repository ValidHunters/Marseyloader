using System;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using ReactiveUI;
using SS14.Launcher.Models;
using SS14.Launcher.Models.Logins;
using SS14.Launcher.Models.ServerStatus;

namespace SS14.Launcher.ViewModels.MainWindowTabs
{
    public sealed class ServerEntryViewModel : ViewModelBase
    {
        private static readonly Color ColorAltBackground = Color.Parse("#262626");

        private readonly ServerStatusCache _cache;
        private readonly IServerStatusData _cacheData;
        private readonly DataManager _cfg;
        private readonly Updater _updater;
        private readonly LoginManager _loginMgr;
        private bool _isAltBackground;
        private string Address => _cacheData.Address;
        private string _fallbackName = string.Empty;

        public ServerEntryViewModel(ServerStatusCache cache, DataManager cfg, Updater updater, LoginManager loginMgr, string address)
        {
            _cache = cache;
            _cacheData = cache.GetStatusFor(address);
            _cfg = cfg;
            _updater = updater;
            _loginMgr = loginMgr;

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

        public ServerEntryViewModel(ServerStatusCache cache, DataManager cfg, Updater updater, LoginManager loginMgr,
            FavoriteServer favorite)
            : this(cache, cfg, updater, loginMgr, favorite.Address)
        {
            Favorite = favorite;
        }

        public Control? Control { get; set; }

        public void DoInitialUpdate()
        {
            _cache.InitialUpdateStatus(_cacheData);
        }

        public void ConnectPressed()
        {
            ConnectingViewModel.StartConnect(_updater, _cfg, _loginMgr, (Window) Control?.GetVisualRoot()!, Address);
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
}
