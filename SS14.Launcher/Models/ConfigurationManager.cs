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
        private readonly object _configWriteLock = new object();

        private readonly SourceCache<FavoriteServer, string> _favoriteServers
            = new SourceCache<FavoriteServer, string>(f => f.Address);

        private bool _ignoreSave;
        private string? _userName;
        private int? _currentBuild;

        public ConfigurationManager()
        {
            // Save when anything about the favorite servers list changes.
            _favoriteServers
                .Connect()
                .WhenAnyPropertyChanged()
                .Subscribe(_ => Save());

            _favoriteServers.Connect()
                .Subscribe(_ => Save());
        }

        /// <summary>
        ///     The username used to log into servers.
        /// </summary>
        public string? UserName
        {
            get => _userName;
            set
            {
                this.RaiseAndSetIfChanged(ref _userName, value);
                Save();
            }
        }

        /// <summary>
        ///     The build number of the current downloaded build.
        /// </summary>
        public int? CurrentBuild
        {
            get => _currentBuild;
            set
            {
                this.RaiseAndSetIfChanged(ref _currentBuild, value);
                Save();
            }
        }

        public IObservableCache<FavoriteServer, string> FavoriteServers => _favoriteServers;

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
                    _favoriteServers.Edit(x =>
                    {
                        x.Clear();
                        x.AddOrUpdate(
                            new FavoriteServer("Wizard's Den", "ss14s://builds.spacestation14.io/ss14_server"));
                        x.AddOrUpdate(new FavoriteServer("Honk", "ss14s://server.spacestation14.io"));
                    });
                    UserName = null;
                    return;
                }

                var data = JsonConvert.DeserializeObject<JsonData>(File.ReadAllText(path));

                UserName = data.Username;
                CurrentBuild = data.Build;

                _favoriteServers.Edit(a =>
                {
                    a.Clear();
                    a.AddOrUpdate(data.Favorites);
                });
            }
            finally
            {
                _ignoreSave = false;
            }
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
                Username = _userName,
                Favorites = _favoriteServers.Items.ToList(),
                Build = CurrentBuild
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
            [JsonProperty(PropertyName = "username")]
            public string? Username { get; set; }

            [JsonProperty(PropertyName = "favorites")]
            public List<FavoriteServer>? Favorites { get; set; }

            [JsonProperty(PropertyName = "build")] public int? Build { get; set; }
        }
    }
}