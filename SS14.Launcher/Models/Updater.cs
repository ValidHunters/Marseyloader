using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.EngineManager;

namespace SS14.Launcher.Models
{
    public sealed class Updater : ReactiveObject
    {
        private readonly DataManager _cfg;
        private readonly IEngineManager _engineManager;
        private bool _updating;

        public Updater(DataManager cfg, IEngineManager engineManager)
        {
            _cfg = cfg;
            _engineManager = engineManager;
        }

        [Reactive] public UpdateStatus Status { get; private set; }
        [Reactive] public (long downloaded, long total)? Progress { get; private set; }

        public async Task<InstalledServerContent?> RunUpdateForLaunchAsync(ServerBuildInformation buildInformation)
        {
            if (_updating)
            {
                throw new InvalidOperationException("Update already in progress.");
            }

            _updating = true;

            try
            {
                var install = await RunUpdate(buildInformation);
                Status = UpdateStatus.Ready;
                return install;
            }
            catch (Exception e)
            {
                Status = UpdateStatus.Error;
                Log.Error(e, "Exception while trying to run updates");
            }
            finally
            {
                _updating = false;
            }

            return null;
        }

        private async Task<InstalledServerContent> RunUpdate(ServerBuildInformation buildInformation)
        {
            Status = UpdateStatus.CheckingClientUpdate;

            var changeEngine = await InstallEngineVersionIfMissing(buildInformation.EngineVersion);

            Status = UpdateStatus.CheckingClientUpdate;

            var (installation, changedContent) = await UpdateContentIfNecessary(buildInformation);

            if (changedContent || changeEngine)
            {
                Status = UpdateStatus.CullingEngine;
                await CullEngineVersionsMaybe();
            }

            Log.Information("Update done!");
            return installation;
        }

        private async Task<(InstalledServerContent, bool changed)> UpdateContentIfNecessary(
            ServerBuildInformation buildInformation)
        {
            if (!CheckNeedUpdate(buildInformation, out var existingInstallation))
            {
                return (existingInstallation, false);
            }

            Status = UpdateStatus.DownloadingClientUpdate;

            var diskId = existingInstallation?.DiskId ?? _cfg.GetNewInstallationId();
            var binPath = Path.Combine(LauncherPaths.DirServerContent,
                diskId.ToString(CultureInfo.InvariantCulture) + ".zip");

            Helpers.EnsureDirectoryExists(LauncherPaths.DirServerContent);
            await using var file = File.Create(binPath, 4096, FileOptions.Asynchronous);

            await Global.GlobalHttpClient.DownloadToStream(
                buildInformation.DownloadUrl,
                file,
                DownloadProgressCallback);

            file.Position = 0;

            Progress = null;

            Status = UpdateStatus.Verifying;

            if (buildInformation.Hash != null)
            {
                var hash = await Task.Run(() => HashFile(file));
                file.Position = 0;

                var expectHash = buildInformation.Hash;

                var newFileHashString = ByteArrayToString(hash);
                if (!expectHash.Equals(newFileHashString, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Hash mismatch. Expected: {expectHash}, got: {newFileHashString}");
                }
            }

            // Write version to disk.
            string? oldEngineVersion = null;
            if (existingInstallation != null)
            {
                oldEngineVersion = existingInstallation.CurrentEngineVersion;
                existingInstallation.CurrentVersion = buildInformation.Version;
                existingInstallation.CurrentHash = buildInformation.Hash;
                existingInstallation.CurrentEngineVersion = buildInformation.EngineVersion;
            }
            else
            {
                existingInstallation = new InstalledServerContent(
                    buildInformation.Version,
                    buildInformation.Hash,
                    buildInformation.ForkId,
                    diskId,
                    buildInformation.EngineVersion);
                _cfg.AddInstallation(existingInstallation);
            }

            return (existingInstallation, true);
        }

        private async Task CullEngineVersionsMaybe()
        {
            await _engineManager.DoEngineCullMaybeAsync();
        }

        private async Task<bool> InstallEngineVersionIfMissing(string engineVer)
        {
            Status = UpdateStatus.DownloadingEngineVersion;
            var change = await _engineManager.DownloadEngineIfNecessary(engineVer, DownloadProgressCallback);

            Progress = null;
            return change;
        }

        private void DownloadProgressCallback(long downloaded, long total)
        {
            Dispatcher.UIThread.Post(() => Progress = (downloaded, total));
        }

        internal static byte[] HashFile(Stream stream)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(stream);
        }

        // https://stackoverflow.com/a/311179/4678631
        public static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        private bool CheckNeedUpdate(
            ServerBuildInformation buildInfo,
            [NotNullWhen(false)] out InstalledServerContent? installation)
        {
            var existingInstallation = _cfg.ServerContent.Lookup(buildInfo.ForkId);
            if (existingInstallation.HasValue)
            {
                installation = existingInstallation.Value;
                var currentVersion = existingInstallation.Value.CurrentVersion;
                if (buildInfo.Version != currentVersion)
                {
                    Log.Information("Current version ({currentVersion}) is out of date, updating to {newVersion}.",
                        currentVersion, buildInfo.Version);

                    return true;
                }

                // Check hash.
                var currentHash = existingInstallation.Value.CurrentHash;
                if (buildInfo.Hash != null && !buildInfo.Hash.Equals(currentHash, StringComparison.OrdinalIgnoreCase))
                {
                    Log.Information("Hash mismatch, re-downloading.");
                    return true;
                }

                return false;
            }

            Log.Information("As it turns out, we don't have any version yet. Time to update.");

            installation = null;
            return true;
        }

        public enum UpdateStatus
        {
            CheckingClientUpdate,
            DownloadingEngineVersion,
            DownloadingClientUpdate,
            Verifying,
            CullingEngine,
            Ready,
            Error,
        }
    }
}
