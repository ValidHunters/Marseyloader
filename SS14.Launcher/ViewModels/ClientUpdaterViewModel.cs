using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json;
using ReactiveUI;

namespace SS14.Launcher.ViewModels
{
    public sealed class ClientUpdaterViewModel : ViewModelBase
    {
        private const string JenkinsBaseUrl = "https://builds.spacestation14.io/jenkins";
        private const string JenkinsJobName = "SS14 Content";
        private const string CurrentLauncherVersion = "1";

        private readonly HttpClient _httpClient;

        private readonly ObservableAsPropertyHelper<bool> _isProgressIndeterminate;
        private readonly ObservableAsPropertyHelper<string> _statusText;
        private readonly ObservableAsPropertyHelper<bool> _progressVisible;
        private readonly ObservableAsPropertyHelper<string> _launchButtonText;
        private readonly ObservableAsPropertyHelper<bool> _launchButtonEnabled;

        private float _progress;
        private UpdateStatus _status;

        public ClientUpdaterViewModel()
        {
            this.WhenAnyValue(x => x.Progress)
                .Select(x => x <= 0)
                .ToProperty(this, x => x.IsProgressIndeterminate, out _isProgressIndeterminate);

            this.WhenAnyValue(x => x.Status)
                .Select(TextForStatus)
                .ToProperty(this, x => x.StatusText, out _statusText);

            this.WhenAnyValue(x => x.Status)
                .Select(ProgressVisibleForStatus)
                .ToProperty(this, x => x.ProgressVisible, out _progressVisible);

            this.WhenAnyValue(x => x.Status)
                .Select(LaunchButtonEnabledForStatus)
                .ToProperty(this, x => x.LaunchButtonEnabled, out _launchButtonEnabled);

            this.WhenAnyValue(x => x.Status)
                .Select(LaunchButtonTextForStatus)
                .ToProperty(this, x => x.LaunchButtonText, out _launchButtonText);

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", $"SS14.Launcher v{CurrentLauncherVersion}");
        }

        public void LaunchButtonPressed()
        {
            if (Status == UpdateStatus.Ready)
            {
                LaunchClient();
            }

            else if (Status == UpdateStatus.LauncherOutOfDate)
            {
                Helpers.OpenUri(new Uri("https://spacestation14.io/about/nightlies/"));
            }
        }

        public float Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        public UpdateStatus Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        public string StatusText => _statusText.Value;
        public bool LaunchButtonEnabled => _launchButtonEnabled.Value;
        public string LaunchButtonText => _launchButtonText.Value;
        public bool ProgressVisible => _progressVisible.Value;

        public bool IsProgressIndeterminate => _isProgressIndeterminate.Value;

        public enum UpdateStatus
        {
            CheckingLauncherUpdate,
            LauncherOutOfDate,
            CheckingClientUpdate,
            DownloadingClientUpdate,
            Extracting,
            Ready,
            Error
        }

        private static string TextForStatus(UpdateStatus status)
        {
            switch (status)
            {
                case UpdateStatus.CheckingLauncherUpdate:
                    return "Checking for launcher update...";
                case UpdateStatus.LauncherOutOfDate:
                    return "This launcher is out of date.";
                case UpdateStatus.CheckingClientUpdate:
                    return "Checking for client update...";
                case UpdateStatus.DownloadingClientUpdate:
                    return "Downloading client update...";
                case UpdateStatus.Extracting:
                    return "Extracting update...";
                case UpdateStatus.Ready:
                    return "Ready!";
                case UpdateStatus.Error:
                    return "An error occured!\nMake sure you can access builds.spacestation14.io";
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        private static bool LaunchButtonEnabledForStatus(UpdateStatus status)
        {
            return status == UpdateStatus.LauncherOutOfDate || status == UpdateStatus.Ready;
        }

        private static string LaunchButtonTextForStatus(UpdateStatus status)
        {
            switch (status)
            {
                case UpdateStatus.CheckingLauncherUpdate:
                case UpdateStatus.CheckingClientUpdate:
                case UpdateStatus.DownloadingClientUpdate:
                case UpdateStatus.Extracting:
                    return "Updating..";
                case UpdateStatus.Ready:
                    return "Launch!";
                case UpdateStatus.LauncherOutOfDate:
                    return "Download Update";
                case UpdateStatus.Error:
                    return "Error!";
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        private static bool ProgressVisibleForStatus(UpdateStatus status)
        {
            switch (status)
            {
                case UpdateStatus.CheckingLauncherUpdate:
                case UpdateStatus.Extracting:
                case UpdateStatus.CheckingClientUpdate:
                case UpdateStatus.DownloadingClientUpdate:
                    return true;
                case UpdateStatus.LauncherOutOfDate:
                case UpdateStatus.Ready:
                case UpdateStatus.Error:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        private async void RunUpdateChecks()
        {
            try
            {
                await Task.Delay(2000);
                Status = UpdateStatus.CheckingLauncherUpdate;
                var needsUpdate = await NeedLauncherUpdate();
                if (needsUpdate)
                {
                    Status = UpdateStatus.LauncherOutOfDate;
                    return;
                }

                await RunUpdate();

                Status = UpdateStatus.Ready;
            }
            catch (Exception e)
            {
                Status = UpdateStatus.Error;
                Console.WriteLine("Exception while trying to run updates:\n{0}", e);
            }
        }

        private async Task<bool> NeedLauncherUpdate()
        {
            var launcherVersionUri =
                new Uri($"{JenkinsBaseUrl}/userContent/current_launcher_version.txt");
            var versionRequest = await _httpClient.GetAsync(launcherVersionUri);
            versionRequest.EnsureSuccessStatusCode();
            return CurrentLauncherVersion != (await versionRequest.Content.ReadAsStringAsync()).Trim();
        }

        private async Task RunUpdate()
        {
            Status = UpdateStatus.CheckingClientUpdate;

            var jobUri = new Uri($"{JenkinsBaseUrl}/job/{Uri.EscapeUriString(JenkinsJobName)}/api/json");
            var jobDataResponse = await _httpClient.GetAsync(jobUri);
            if (!jobDataResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Got bad status code {jobDataResponse.StatusCode} from Jenkins.");
            }

            var jobInfo = JsonConvert.DeserializeObject<JenkinsJobInfo>(
                await jobDataResponse.Content.ReadAsStringAsync());
            var latestBuildNumber = jobInfo.LastSuccessfulBuild.Number;

            var versionFile = Path.Combine(UserDataDir.GetUserDataDir(), "current_build");
            bool needUpdate;
            if (File.Exists(versionFile))
            {
                var buildNumber = int.Parse(File.ReadAllText(versionFile, Encoding.UTF8),
                    CultureInfo.InvariantCulture);
                needUpdate = buildNumber != latestBuildNumber;
                if (needUpdate)
                {
                    Console.WriteLine("Current version ({0}) is out of date, updating to {1}.", buildNumber,
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
            Progress = -1;

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
                    Arguments = $"+x '{Path.Combine("Space Station 14.app", "Contents", "MacOS", "SS14")}'",
                    WorkingDirectory = binPath,
                });
                process?.WaitForExit();
            }

            // Write version to disk.
            File.WriteAllText(versionFile, latestBuildNumber.ToString(CultureInfo.InvariantCulture),
                new UTF8Encoding(false));

            Console.WriteLine("Update done!");
        }

        private async Task<string> DownloadArtifactToTempFile(int buildNumber, string fileName)
        {
            var artifactUri
                = new Uri(
                    $"{JenkinsBaseUrl}/job/{Uri.EscapeUriString(JenkinsJobName)}/{buildNumber}/artifact/release/{Uri.EscapeUriString(fileName)}");

            var tmpFile = Path.GetTempFileName();
            await _httpClient.DownloadToFile(artifactUri, tmpFile, f => Dispatcher.UIThread.Post(() => Progress = f));

            return tmpFile;
        }

        private void LaunchClient()
        {
            var binPath = Path.Combine(UserDataDir.GetUserDataDir(), "client_bin");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "mono",
                        Arguments = "Robust.Client.exe",
                        WorkingDirectory = binPath,
                        UseShellExecute = false,
                    },
                };
                process.Start();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Path.Combine(binPath, "Robust.Client.exe"),
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // TODO: does this cause macOS to make a security warning?
                // If it does we'll have to manually launch the contents, which is simple enough.
                Process.Start(new ProcessStartInfo
                {
                    FileName = "open",
                    Arguments = "'Space Station 14.app'",
                    WorkingDirectory = binPath,
                });
            }
            else
            {
                throw new NotSupportedException("Unsupported platform.");
            }
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

        public void OnWindowInitialized()
        {
            RunUpdateChecks();
        }
    }
}