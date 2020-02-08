using System;
using System.Runtime.InteropServices;
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

        [JsonProperty(PropertyName = "build")]
        public ServerBuildInformation? BuildInformation { get; set; }
    }

    [JsonObject(ItemRequired = Required.Always)]
    public class ServerBuildInformation
    {
        [JsonProperty(PropertyName = "download_urls")]
        public PlatformList DownloadUrls { get; set; } = default!;

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; } = default!;

        [JsonProperty(PropertyName = "fork_id")]
        public string ForkId { get; set; } = default!;

        [JsonProperty(PropertyName = "hashes")]
        public PlatformList Hashes { get; set; } = default!;
    }

    [JsonObject]
    public class PlatformList
    {
        public string? Windows { get; set; }
        public string? Linux { get; set; }
        public string? MacOS { get; set; }

        public string? ForCurrentPlatform
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return Windows;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return Linux;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return MacOS;
                }

                throw new PlatformNotSupportedException();
            }
        }
    }
}