using Newtonsoft.Json;
using ReactiveUI;

namespace SS14.Launcher.Models
{
    // Without OptIn JSON.NET chokes on ReactiveObject.
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Installation : ReactiveObject
    {
        [JsonProperty(PropertyName = "current_version")]
        private string _currentVersion;

        [JsonProperty(PropertyName = "current_hash")]
        private string? _currentHash;

        public Installation(string currentVersion, string? currentHash, string forkId, int diskId)
        {
            _currentVersion = currentVersion;
            _currentHash = currentHash;
            ForkId = forkId;
            DiskId = diskId;
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
    }
}