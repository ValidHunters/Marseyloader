using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
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

        public async Task<bool> DownloadEngineIfNecessary(
            string engineVersion,
            Helpers.DownloadProgressCallback? progress = null,
            CancellationToken cancel = default)
        {
            if (_cfg.EngineInstallations.Lookup(engineVersion).HasValue)
            {
                // Already have the engine version, we're good.
                return false;
            }

            Log.Information("Installing engine version {version}...", engineVersion);

            Log.Debug("Loading manifest from {manifestUrl}...", ConfigConstants.RobustBuildsManifest);
            var manifest =
                await Global.GlobalHttpClient.GetFromJsonAsync<Dictionary<string, Dictionary<string, BuildInfo>>>(
                    ConfigConstants.RobustBuildsManifest, cancellationToken: cancel);

            if (!manifest!.TryGetValue(engineVersion, out var versionInfo))
            {
                throw new UpdateException("Unable to find engine version in manifest!");
            }

            var bestRid = RidUtility.FindBestRid(versionInfo.Keys);
            if (bestRid == null)
            {
                throw new UpdateException("No engine version available for our platform!");
            }

            Log.Debug("Selecting RID {rid}", bestRid);

            var buildInfo = versionInfo[bestRid];

            var downloadTarget = Path.Combine(LauncherPaths.DirEngineInstallations, $"{engineVersion}.zip");
            await using var file = File.Create(downloadTarget, 4096, FileOptions.Asynchronous);

            Helpers.EnsureDirectoryExists(LauncherPaths.DirEngineInstallations);

            try
            {
                await Global.GlobalHttpClient.DownloadToStream(buildInfo.Url, file, progress, cancel: cancel);
            }
            catch (OperationCanceledException)
            {
                // Don't leave behind garbage.
                await file.DisposeAsync();
                File.Delete(downloadTarget);

                throw;
            }

            _cfg.AddEngineInstallation(new InstalledEngineVersion(engineVersion, buildInfo.Signature));
            return true;
        }

        public async Task DoEngineCullMaybeAsync()
        {
            Log.Debug("Checking to cull engine versions.");

            var usedVersions = _cfg.ServerContent.Items.Select(c => c.CurrentEngineVersion).ToHashSet();
            var toCull = _cfg.EngineInstallations.Items.Where(i => !usedVersions.Contains(i.Version)).ToArray();

            foreach (var installation in toCull)
            {
                Log.Debug("Culling unused version {engineVersion}", installation.Version);

                var path = GetEnginePath(installation.Version);

                _cfg.RemoveEngineInstallation(installation);

                await Task.Run(() => File.Delete(path));
            }
        }

        public void ClearAllEngines()
        {
            foreach (var install in _cfg.EngineInstallations.Items.ToArray())
            {
                _cfg.RemoveEngineInstallation(install);
            }

            foreach (var file in Directory.EnumerateFiles(LauncherPaths.DirEngineInstallations))
            {
                File.Delete(file);
            }
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
