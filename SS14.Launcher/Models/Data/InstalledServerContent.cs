using Newtonsoft.Json;
using ReactiveUI;

namespace SS14.Launcher.Models.Data
{
    // Without OptIn JSON.NET chokes on ReactiveObject.
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class InstalledServerContent : ReactiveObject
    {
        [JsonProperty(PropertyName = "current_version")]
        private string _currentVersion;

        [JsonProperty(PropertyName = "current_hash")]
        private string? _currentHash;

        [JsonProperty(PropertyName = "current_engine_version")]
        private string _currentEngineVersion;

        public InstalledServerContent(
            string currentVersion,
            string? currentHash,
            string forkId,
            int diskId,
            string currentEngineVersion)
        {
            _currentVersion = currentVersion;
            _currentHash = currentHash;
            ForkId = forkId;
            DiskId = diskId;
            _currentEngineVersion = currentEngineVersion;
        }

        [JsonProperty(PropertyName = "fork_id")]
        public string ForkId { get; private set; } // Private set for JSON.NET

        [JsonProperty(PropertyName = "disk_id")]
        public int DiskId { get; private set; } // Ditto

        public string CurrentVersion
        {
            get => _currentVersion;
            set => this.RaiseAndSetIfChanged(ref _currentVersion, value);
        }

        public string? CurrentHash
        {
            get => _currentHash;
            set => this.RaiseAndSetIfChanged(ref _currentHash, value);
        }

        public string CurrentEngineVersion
        {
            get => _currentEngineVersion;
            set => this.RaiseAndSetIfChanged(ref _currentEngineVersion, value);
        }
    }
}
