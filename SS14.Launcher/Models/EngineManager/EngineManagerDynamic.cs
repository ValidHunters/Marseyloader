using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Serilog;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Utility;

namespace SS14.Launcher.Models.EngineManager
{
    /// <summary>
    ///     Downloads engine versions from the website.
    /// </summary>
    public sealed class EngineManagerDynamic : IEngineManager
    {
        private readonly DataManager _cfg;

        public EngineManagerDynamic(DataManager cfg)
        {
            _cfg = cfg;
        }

        public string GetEnginePath(string engineVersion)
        {
            if (!_cfg.EngineInstallations.Lookup(engineVersion).HasValue)
            {
                throw new ArgumentException("We do not have that engine version!");
            }

            return Path.Combine(LauncherPaths.DirEngineInstallations, $"{engineVersion}.zip");
        }

        public string GetEngineSignature(string engineVersion)
        {
            return _cfg.EngineInstallations.Lookup(engineVersion).Value.Signature;
        }

        public async Task DownloadEngineIfNecessary(string engineVersion,
            Helpers.DownloadProgressCallback? progress = null)
        {
            if (_cfg.EngineInstallations.Lookup(engineVersion).HasValue)
            {
                // Already have the engine version, we're good.
                return;
            }

            Log.Information("Installing engine version {version}...", engineVersion);

            Log.Debug("Loading manifest from {manifestUrl}...", ConfigConstants.RobustBuildsManifest);
            var manifest =
                await Global.GlobalHttpClient.GetFromJsonAsync<Dictionary<string, Dictionary<string, BuildInfo>>>(
                    ConfigConstants.RobustBuildsManifest);

            if (!manifest!.TryGetValue(engineVersion, out var versionInfo))
            {
                throw new UpdateException("Unable to find engine version in manifest!");
            }

            var rid = RidUtility.GetRid();
            Log.Debug("Current RID is {rid}", rid);
            var bestRid = RidUtility.FindBestRid(versionInfo.Keys, rid);
            if (bestRid == null)
            {
                throw new UpdateException("No engine version available for our platform!");
            }

            Log.Debug("Selecting RID {rid}", bestRid);

            var buildInfo = versionInfo[bestRid];

            var downloadTarget = Path.Combine(LauncherPaths.DirEngineInstallations, $"{engineVersion}.zip");
            await using var file = File.Create(downloadTarget, 4096, FileOptions.Asynchronous);

            Helpers.EnsureDirectoryExists(LauncherPaths.DirEngineInstallations);

            await Global.GlobalHttpClient.DownloadToStream(buildInfo.Url, file, progress);

            _cfg.AddEngineInstallation(new InstalledEngineVersion(engineVersion, buildInfo.Signature));
        }

        public async Task DoEngineCullMaybeAsync(string engineVersion)
        {
            var lookup = _cfg.EngineInstallations.Lookup(engineVersion);
            Debug.Assert(lookup.HasValue);

            // Check if the engine version is no longer used by any server install and if so, remove it.
            Log.Debug("Checking cull for engine {engineVersion}", engineVersion);

            foreach (var item in _cfg.ServerContent.Items)
            {
                if (item.CurrentEngineVersion == engineVersion)
                {
                    Log.Debug("Engine version still in use by {forkId}v{version}, not culling", item.ForkId,
                        item.CurrentVersion);
                    return;
                }
            }

            var path = GetEnginePath(engineVersion);

            _cfg.RemoveEngineInstallation(lookup.Value);

            await Task.Run(() => File.Delete(path));
        }

        private sealed class BuildInfo
        {
            [JsonInclude] [JsonPropertyName("url")]
            public string Url = default!;

            [JsonInclude] [JsonPropertyName("sha256")]
            public string Sha256 = default!;

            [JsonInclude] [JsonPropertyName("sig")]
            public string Signature = default!;
        }
    }
}
