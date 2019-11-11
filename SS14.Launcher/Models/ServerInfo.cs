using Newtonsoft.Json;

namespace SS14.Launcher.Models
{
    /// <summary>
    ///     Basic data type to deserialize from the /info API endpoint on the server.
    /// </summary>
    public sealed class ServerInfo
    {
        [JsonProperty(PropertyName = "connect_address")]
        public string? ConnectAddress { get; set; }
    }
}