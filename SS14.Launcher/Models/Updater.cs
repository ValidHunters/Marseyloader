using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Threading;
using Newtonsoft.Json;
using ReactiveUI;

namespace SS14.Launcher.Models
{
    public sealed class Updater : ReactiveObject
    {
        private readonly ConfigurationManager _cfg;
        private float? _progress;
        private UpdateStatus _status;
        private Task? _updateDoneTask;

        public Updater(ConfigurationManager cfg)
        {
            _cfg = cfg;
        }

        public UpdateStatus Status
        {
            get => _status;
            private set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        public float? Progress
        {
            get => _progress;
            private set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        public Task RunUpdatesAsync()
        {
            if (_updateDoneTask == null)
            {
                _updateDoneTask = RunUpdatesInternal();
            }

            return _updateDoneTask;
        }

        private async Task RunUpdatesInternal()
        {
            try
            {
                await RunUpdate();

                Status = UpdateStatus.Ready;
            }
            catch (Exception e)
            {
                Status = UpdateStatus.Error;
                Console.WriteLine("Exception while trying to run updates:\n{0}", e);
            }
        }

        private async Task RunUpdate()
        {
            Status = UpdateStatus.CheckingClientUpdate;

            var jobUri = new Uri($"{Global.JenkinsBaseUrl}/job/{Uri.EscapeUriString(Global.JenkinsJobName)}/api/json");
            var jobDataResponse = await Global.GlobalHttpClient.GetAsync(jobUri);
            if (!jobDataResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Got bad status code {jobDataResponse.StatusCode} from Jenkins.");
            }

            var jobInfo = JsonConvert.DeserializeObject<JenkinsJobInfo>(
                await jobDataResponse.Content.ReadAsStringAsync());
            var latestBuildNumber = jobInfo!.LastSuccessfulBuild!.Number;


            bool needUpdate;
            if (_cfg.CurrentBuild != null)
            {
                needUpdate = _cfg.CurrentBuild.Value != latestBuildNumber;
                if (needUpdate)
                {
                    Console.WriteLine("Current version ({0}) is out of date, updating to {1}.", _cfg.CurrentBuild.Value,
                        latestBuildNumber);
                }
            }
            else
            {
                Console.WriteLine("As it turns out, we don't have any version yet. Time to update.");
                // Version file doesn't exist, assume first run or whatever.
                needUpdate = true;
            }

            if (!needUpdate)
            {
                Console.WriteLine("No update needed!");
                return;
            }

            Status = UpdateStatus.DownloadingClientUpdate;
            var binPath = Path.Combine(UserDataDir.GetUserDataDir(), "client_bin");

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

            // We download the artifact to a temporary file on disk.
            // This is to avoid having to load the entire thing into memory.
            // (.NET's zip code loads it into a memory stream if the stream you give it doesn't support seeking.)
            // (this makes a lot of sense due to how the zip file format works.)
            var tmpFile = await DownloadArtifactToTempFile(latestBuildNumber, GetBuildFilename());
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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // .NET's zip extraction system doesn't seem to preserve +x.
                // Technically can't blame it because there's no "official" way to store that,
                // since zip files are DOS-centric.

                // Manually chmod +x the App bundle then.
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "chmod",
                    ArgumentList =
                    {
                        "+x",
                        Path.Combine("Space Station 14.app", "Contents", "MacOS", "SS14")
                    },
                    WorkingDirectory = binPath,
                });
                process?.WaitForExit();
            }

            // Write version to disk.
            _cfg.CurrentBuild = latestBuildNumber;

            Console.WriteLine("Update done!");
        }

        private async Task<string> DownloadArtifactToTempFile(int buildNumber, string fileName)
        {
            var artifactUri
                = new Uri(
                    $"{Global.JenkinsBaseUrl}/job/{Uri.EscapeUriString(Global.JenkinsJobName)}/{buildNumber}/artifact/release/{Uri.EscapeUriString(fileName)}");

            var tmpFile = Path.GetTempFileName();
            await Global.GlobalHttpClient.DownloadToFile(artifactUri, tmpFile,
                f => Dispatcher.UIThread.Post(() => Progress = f));

            return tmpFile;
        }

        [Pure]
        private static string GetBuildFilename()
        {
            string platform;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platform = "Windows";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                platform = "Linux";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                platform = "macOS";
            }
            else
            {
                throw new NotSupportedException("Unsupported platform.");
            }

            return $"SS14.Client_{platform}_x64.zip";
        }

        public enum UpdateStatus
        {
            CheckingClientUpdate,
            DownloadingClientUpdate,
            Extracting,
            Ready,
            Error
        }
    }
}
