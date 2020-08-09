using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using Newtonsoft.Json;
using ReactiveUI;

namespace SS14.Launcher.Models
{
    /// <summary>
    ///     Handles storage of all permanent data,
    ///     like username, current build, favorite servers...
    /// </summary>
    public sealed class ConfigurationManager : ReactiveObject
    {
        private static readonly (string name, string addr)[] DefaultFavorites =
        {
            ("Wizard's Den [US West]", "ss14s://builds.spacestation14.io/ss14_server")
        };

        private readonly object _configWriteLock = new object();

        private readonly SourceCache<FavoriteServer, string> _favoriteServers
            = new SourceCache<FavoriteServer, string>(f => f.Address);

        private readonly SourceCache<Installation, string> _installations
            = new SourceCache<Installation, string>(i => i.ForkId);

        private readonly SourceCache<LoginInfo, Guid> _logins
            = new SourceCache<LoginInfo, Guid>(l => l.UserId);

        private bool _ignoreSave = true;
        private int _nextInstallationId = 1;
        private Guid _fingerprint;
        private Guid? _selectedLogin;

        public ConfigurationManager()
        {
            // Save when anything about the favorite servers list changes.
            _favoriteServers
                .Connect()
                .WhenAnyPropertyChanged()
                .Subscribe(_ => Save());

            _favoriteServers.Connect()
                .Subscribe(_ => Save());

            // Also the installations list.
            _installations
                .Connect()
                .WhenAnyPropertyChanged()
                .Subscribe(_ => Save());

            _installations.Connect()
                .Subscribe(_ => Save());

            _logins.Connect()
                .Subscribe(_ => Save());
        }

        public Guid Fingerprint => _fingerprint;

        public Guid? SelectedLoginId
        {
            get => _selectedLogin;
            set
            {
                if (value != null && !_logins.Lookup(value.Value).HasValue)
                {
                    throw new ArgumentException("We are not logged in for that user ID.");
                }

                this.RaiseAndSetIfChanged(ref _selectedLogin, value, nameof(SelectedLoginId));
                this.RaisePropertyChanged(nameof(SelectedLogin));
                Save();
            }
        }

        public LoginInfo? SelectedLogin
        {
            get
            {
                if (_selectedLogin == null)
                {
                    return null;
                }

                return _logins.Lookup(_selectedLogin.Value).Value;
            }
        }

        public IObservableCache<FavoriteServer, string> FavoriteServers => _favoriteServers;
        public IObservableCache<Installation, string> Installations => _installations;
        public IObservableCache<LoginInfo, Guid> Logins => _logins;

        public void AddFavoriteServer(FavoriteServer server)
        {
            if (_favoriteServers.Lookup(server.Address).HasValue)
            {
                throw new ArgumentException("A server with that address is already a favorite.");
            }

            _favoriteServers.AddOrUpdate(server);
        }

        public void RemoveFavoriteServer(FavoriteServer server)
        {
            _favoriteServers.Remove(server);
        }

        public void AddInstallation(Installation installation)
        {
            if (_favoriteServers.Lookup(installation.ForkId).HasValue)
            {
                throw new ArgumentException("An installation with that fork ID already exists.");
            }

            _installations.AddOrUpdate(installation); // Will do a save.
        }

        public void RemoveInstallation(Installation installation)
        {
            _installations.Remove(installation);
        }

        public void AddLogin(LoginInfo login)
        {
            if (_logins.Lookup(login.UserId).HasValue)
            {
                throw new ArgumentException("A login with that UID already exists.");
            }

            _logins.AddOrUpdate(login);
        }

        public void RemoveLogin(LoginInfo loginInfo)
        {
            _logins.Remove(loginInfo);

            if (loginInfo.UserId == _selectedLogin)
            {
                SelectedLoginId = null;
            }
        }

        public int GetNewInstallationId()
        {
            // Don't explicitly save.
            // If something is actually gonna use this installation ID it'll cause a save.
            return _nextInstallationId++;
        }

        /// <summary>
        ///     Loads config file from disk, or resets the loaded config to default if the config doesn't exist on disk.
        /// </summary>
        public void Load()
        {
            _ignoreSave = true;
            try
            {
                var path = GetCfgPath();

                if (!File.Exists(path))
                {
                    LoadDefaultConfig();
                    return;
                }

                using var changeSuppress = SuppressChangeNotifications();

                var data = JsonConvert.DeserializeObject<JsonData>(File.ReadAllText(path));

                _nextInstallationId = data.NextInstallationId;

                _favoriteServers.Edit(a =>
                {
                    a.Clear();
                    a.AddOrUpdate(data.Favorites);
                });

                _logins.Edit(p =>
                {
                    p.Clear();
                    if (data.Logins != null)
                    {
                        p.AddOrUpdate(data.Logins);
                    }
                });

                if (data.Installations != null)
                {
                    _installations.Edit(a =>
                    {
                        a.Clear();
                        a.AddOrUpdate(data.Installations);
                    });
                }

                _fingerprint = data.Fingerprint;
                _selectedLogin = data.SelectedLogin;
            }
            finally
            {
                _ignoreSave = false;
            }

            if (_fingerprint == default)
            {
                // If we don't have a fingerprint yet this is either a fresh config or an older config.
                // Generate a fingerprint and immediately save it to disk.
                _fingerprint = Guid.NewGuid();
                Save();
            }
        }

        private void LoadDefaultConfig()
        {
            _favoriteServers.Edit(x =>
            {
                x.Clear();
                x.AddOrUpdate(DefaultFavorites.Select(p => new FavoriteServer(p.name, p.addr)));
            });
        }

        private void Save()
        {
            if (_ignoreSave)
            {
                return;
            }

            var path = GetCfgPath();

            var data = JsonConvert.SerializeObject(new JsonData
            {
                SelectedLogin = _selectedLogin,
                Logins = _logins.Items.ToList(),
                Favorites = _favoriteServers.Items.ToList(),
                NextInstallationId = _nextInstallationId,
                Installations = _installations.Items.ToList(),
                Fingerprint = _fingerprint
            });

            // Save config asynchronously to avoid potential disk hangs.
            Task.Run(() =>
            {
                lock (_configWriteLock)
                {
                    File.WriteAllText(path, data);
                }
            });
        }

        private static string GetCfgPath()
        {
            return Path.Combine(UserDataDir.GetUserDataDir(), "launcher_config.json");
        }

        [Serializable]
        private sealed class JsonData
        {
            [JsonProperty(PropertyName = "selected_login")]
            public Guid? SelectedLogin { get; set; }

            [JsonProperty(PropertyName = "favorites")]
            public List<FavoriteServer>? Favorites { get; set; }

            [JsonProperty(PropertyName = "installations")]
            public List<Installation>? Installations { get; set; }

            [JsonProperty(PropertyName = "logins")]
            public List<LoginInfo>? Logins { get; set; }

            [JsonProperty(PropertyName = "next_installation_id")]
            public int NextInstallationId { get; set; } = 1;

            [JsonProperty(PropertyName = "fingerprint")]
            public Guid Fingerprint { get; set; }
        }
    }
}