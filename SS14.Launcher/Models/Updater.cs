using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using DynamicData.Kernel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace SS14.Launcher.Models
{
    public sealed partial class Updater : ReactiveObject
    {
        private readonly DataManager _cfg;
        private bool _updating = false;

        public Updater(DataManager cfg)
        {
            _cfg = cfg;
        }

        [Reactive] public UpdateStatus Status { get; private set; }
        [Reactive] public (long downloaded, long total)? Progress { get; private set; }

        public async Task<Installation?> RunUpdateForLaunchAsync(ServerBuildInformation? buildInformation)
        {
            if (_updating)
            {
                throw new InvalidOperationException("Update already in progress.");
            }

            _updating = true;

            try
            {
                Status = UpdateStatus.CheckingClientUpdate;
                if (buildInformation == null)
                {
                    buildInformation = await GetDefaultBuildInformation();
                }

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

        private async Task<Installation?> RunUpdatesInternal(ServerBuildInformation? buildInformation)
        {
            try
            {
                Status = UpdateStatus.CheckingClientUpdate;
                if (buildInformation == null)
                {
                    buildInformation = await GetDefaultBuildInformation();
                }

                var install = await RunUpdate(buildInformation);
                Status = UpdateStatus.Ready;
                return install;
            }
            catch (Exception e)
            {
                Status = UpdateStatus.Error;
                Log.Error(e, "Exception while trying to run updates.");
            }

            return null;
        }

        private async Task<Installation> RunUpdate(ServerBuildInformation buildInformation)
        {
            bool needsUpdate;
            var existingInstallation = _cfg.Installations.Lookup(buildInformation.ForkId);
            if (existingInstallation.HasValue)
            {
                var currentVersion = existingInstallation.Value.CurrentVersion;
                if (buildInformation.Version != currentVersion)
                {
                    Log.Information("Current version ({currentVersion}) is out of date, updating to {newVersion}.",
                        currentVersion, buildInformation.Version);

                    needsUpdate = true;
                }
                else
                {
                    // Check SHA.
                    var currentHash = existingInstallation.Value.CurrentHash;
                    if (buildInformation.Hashes.ForCurrentPlatform != null &&
                        currentHash != null &&
                        !currentHash.Equals(buildInformation.Hashes.ForCurrentPlatform,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Information("Hash mismatch, re-downloading anyways.");
                        needsUpdate = true;
                    }
                    else
                    {
                        needsUpdate = false;
                    }
                }
            }
            else
            {
                Log.Information("As it turns out, we don't have any version yet. Time to update.");

                needsUpdate = true;
            }

            if (!needsUpdate)
            {
                return existingInstallation.Value;
            }

            Status = UpdateStatus.DownloadingClientUpdate;

            var diskId = existingInstallation.Convert(x => x.DiskId).ValueOr(() => _cfg.GetNewInstallationId());
            var binPath = Path.Combine(UserDataDir.GetUserDataDir(), "installations",
                diskId.ToString(CultureInfo.InvariantCulture));

            await Task.Run(() =>
            {
                // If your disk feels like being a pain this could actually stutter so..
                // Task.Run() it!
                if (!Directory.Exists(binPath))
                {
                    Directory.CreateDirectory(binPath);
                }
                else
                {
                    Helpers.ClearDirectory(binPath);
                }
            });

            var buildUri = new Uri(buildInformation.DownloadUrls.ForCurrentPlatform!);

            // We download the artifact to a temporary file on disk.
            // This is to avoid having to load the entire thing into memory.
            // (.NET's zip code loads it into a memory stream if the stream you give it doesn't support seeking.)
            // (this makes a lot of sense due to how the zip file format works.)
            var tmpFile = await DownloadArtifactToTempFile(buildUri);

            Status = UpdateStatus.Verifying;

            if (buildInformation.Hashes.ForCurrentPlatform != null)
            {
                var expectHash = buildInformation.Hashes.ForCurrentPlatform;

                var newFileHash = await Task.Run(() =>
                {
                    using var f = File.OpenRead(tmpFile);

                    return HashFile(f);
                });

                var newFileHashString = ByteArrayToString(newFileHash);
                if (!expectHash.Equals(newFileHashString, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Hash mismatch. Expected: {expectHash}, got: {newFileHashString}");
                }
            }

            Status = UpdateStatus.Extracting;
            Progress = null;

            await Task.Run(() =>
            {
                using (var file = File.OpenRead(tmpFile))
                {
                    Helpers.ExtractZipToDirectory(binPath, file);
                }

                File.Delete(tmpFile);
            });

            // .NET's zip extraction system doesn't seem to preserve +x.
            // Technically can't blame it because there's no "official" way to store that,
            // since zip files are DOS-centric.
            // Anyways we have to handle this on macOS and Linux.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Manually chmod +x the App bundle's startup script.
                var processA = Process.Start(new ProcessStartInfo
                {
                    FileName = "chmod",
                    ArgumentList =
                    {
                        "+x",
                        Path.Combine("Space Station 14.app", "Contents", "MacOS", "SS14")
                    },
                    WorkingDirectory = binPath
                });
                // And also chmod +x the Robust.Client executable.
                var processB = Process.Start(new ProcessStartInfo
                {
                    FileName = "chmod",
                    ArgumentList =
                    {
                        "+x",
                        Path.Combine("Space Station 14.app", "Contents", "Resources", "Robust.Client")
                    },
                    WorkingDirectory = binPath
                });

                processA?.WaitForExit();
                processB?.WaitForExit();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "chmod",
                    ArgumentList =
                    {
                        "+x",
                        "Robust.Client"
                    },
                    WorkingDirectory = binPath
                });
                process?.WaitForExit();
            }

            // Write version to disk.
            Installation installation;
            if (existingInstallation.HasValue)
            {
                installation = existingInstallation.Value;
                installation.CurrentVersion = buildInformation.Version;
                installation.CurrentHash = buildInformation.Hashes.ForCurrentPlatform;
            }
            else
            {
                installation = new Installation(buildInformation.Version,
                    buildInformation.Hashes.ForCurrentPlatform, buildInformation.ForkId, diskId);
                _cfg.AddInstallation(installation);
            }

            Log.Information("Update done!");
            return installation;
        }

        private async Task<string> DownloadArtifactToTempFile(Uri downloadUri)
        {
            var tmpFile = Path.GetTempFileName();
            await Global.GlobalHttpClient.DownloadToFile(downloadUri, tmpFile,
                f => Dispatcher.UIThread.Post(() => Progress = f));

            return tmpFile;
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

        public enum UpdateStatus
        {
            CheckingClientUpdate,
            DownloadingClientUpdate,
            Verifying,
            Extracting,
            Ready,
            Error,
        }
    }
}
