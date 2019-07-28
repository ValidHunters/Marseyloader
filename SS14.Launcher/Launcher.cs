using System;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Qml.Net;

namespace SS14.Launcher
{
    public class Launcher
    {
        private const string JenkinsBaseUrl = "https://builds.spacestation14.io/jenkins";
        private const string JenkinsJobName = "SS14 Content";
        private const string CurrentLauncherVersion = "1";

        private readonly HttpClient _httpClient;

        private string _dataDir;

        private LauncherUpdateStatus _status = LauncherUpdateStatus.ClientUpdate;
        private float _progress = -1;

        public Launcher()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", $"SS14.Launcher v{CurrentLauncherVersion}");
        }

        [NotifySignal("statusChanged")]
        [UsedImplicitly]
        public LauncherUpdateStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                this.ActivateSignal("statusChanged");
            }
        }

        [NotifySignal("progressChanged")]
        [UsedImplicitly]
        public float Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                this.ActivateSignal("progressChanged");
            }
        }

        // Used by QML.
        [UsedImplicitly]
        public async Task StartUpdate()
        {
            _dataDir = GetUserDataDir();

            try
            {
                if (await NeedLauncherUpdate())
                {
                    Status = LauncherUpdateStatus.LauncherNeedsUpdate;
                    return;
                }

                await RunUpdate();

                Status = LauncherUpdateStatus.Ready;
                Progress = 1;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception while updating:\n{0}", e.Message);

                Status = LauncherUpdateStatus.Error;
            }
        }

        private async Task<bool> NeedLauncherUpdate()
        {
            Status = LauncherUpdateStatus.LauncherUpdateCheck;

            var launcherVersionUri =
                new Uri("https://builds.spacestation14.io/jenkins/userContent/current_launcher_version.txt");
            var versionRequest = await _httpClient.GetAsync(launcherVersionUri);
            versionRequest.EnsureSuccessStatusCode();
            return CurrentLauncherVersion != (await versionRequest.Content.ReadAsStringAsync()).Trim();
        }

        private async Task RunUpdate()
        {
            Status = LauncherUpdateStatus.ClientUpdateCheck;
            Console.WriteLine("Checking for update...");
            var jobUri = new Uri($"{JenkinsBaseUrl}/job/{Uri.EscapeUriString(JenkinsJobName)}/api/json");
            var jobDataResponse = await _httpClient.GetAsync(jobUri);
            if (!jobDataResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Got bad status code {jobDataResponse.StatusCode} from Jenkins.");
            }

            var jobInfo = JsonConvert.DeserializeObject<JenkinsJobInfo>(
                await jobDataResponse.Content.ReadAsStringAsync());
            var latestBuildNumber = jobInfo.LastSuccessfulBuild.Number;

            var versionFile = Path.Combine(_dataDir, "current_build");
            bool needUpdate;
            if (File.Exists(versionFile))
            {
                var buildNumber = int.Parse(File.ReadAllText(versionFile, Encoding.UTF8), CultureInfo.InvariantCulture);
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

            Status = LauncherUpdateStatus.ClientUpdate;
            var binPath = GetClientBinPath();

            await Task.Run(() =>
            {
                if (!Directory.Exists(binPath))
                {
                    Directory.CreateDirectory(binPath);
                }
                else
                {
                    Helpers.ClearDirectory(binPath);
                }
            });

            var tmpFile = await _downloadArtifactToTempFile(latestBuildNumber, GetBuildFilename());
            Status = LauncherUpdateStatus.ClientUpdateExtracting;
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
            File.WriteAllText(versionFile, latestBuildNumber.ToString(CultureInfo.InvariantCulture), Encoding.UTF8);

            Console.WriteLine("Update done!");
        }

        private async Task<string> _downloadArtifactToTempFile(int buildNumber, string fileName)
        {
            var artifactUri
                = new Uri(
                    $"{JenkinsBaseUrl}/job/{Uri.EscapeUriString(JenkinsJobName)}/{buildNumber}/artifact/release/{Uri.EscapeUriString(fileName)}");

            var tmpFile = Path.GetTempFileName();
            Console.WriteLine(tmpFile);
            using (var response = await _httpClient.GetAsync(artifactUri, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Got bad status code {response.StatusCode} from Jenkins.");
                }

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = File.OpenWrite(tmpFile))
                {
                    var totalLength = response.Content.Headers.ContentLength;
                    if (totalLength.HasValue)
                    {
                        Progress = 0;
                    }

                    var totalRead = 0L;
                    var reads = 0L;
                    const int bufferLength = 40960;
                    var buffer = ArrayPool<byte>.Shared.Rent(bufferLength);
                    var isMoreToRead = true;

                    do
                    {
                        var read = await contentStream.ReadAsync(buffer, 0, bufferLength);
                        if (read == 0)
                        {
                            isMoreToRead = false;
                        }
                        else
                        {
                            await fileStream.WriteAsync(buffer, 0, read);

                            reads += 1;
                            totalRead += read;
                            if (totalLength.HasValue && reads % 20 == 0)
                            {
                                Progress = totalRead / (float)totalLength.Value;
                            }
                        }
                    }
                    while (isMoreToRead);
                }
            }

            return tmpFile;
        }

        // Used by QML.
        [UsedImplicitly]
        public void LaunchClient()
        {
            var binPath = GetClientBinPath();
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


        [System.Diagnostics.Contracts.Pure]
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

        [System.Diagnostics.Contracts.Pure]
        private static string GetClientBinPath()
        {
            return Path.Combine(GetUserDataDir(), "client_bin");
        }

        [System.Diagnostics.Contracts.Pure]
        private static string GetUserDataDir()
        {
            string appDataDir;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // SpecialFolder.ApplicationData is $XDG_CONFIG_HOME on Linux.
                // I'd like $XDG_DATA_HOME. SpecialFolder.LocalApplicationData is but eh.
                // Just do it manually not that hard.
                var xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                if (xdgDataHome == null)
                {
                    var home = Environment.GetEnvironmentVariable("HOME");
                    if (home == null)
                    {
                        throw new Exception("$HOME should always be set on a POSIX system.");
                    }

                    appDataDir = Path.Combine(home, ".local", "share");
                }
                else
                {
                    appDataDir = xdgDataHome;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // TODO: Is SpecialFolder.ApplicationData incorrect on .NET Core on macOS?
                // I know it is on Mono and tries to follow the XDG base directory spec.
                var home = Environment.GetEnvironmentVariable("HOME");
                if (home == null)
                {
                    throw new Exception("$HOME should always be set on a POSIX system.");
                }

                appDataDir = Path.Combine(home, "Library", "Application Support");
            }
            else
            {
                appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }

            return Path.Combine(appDataDir, "Space Station 14", "launcher");
        }
    }

    public enum LauncherUpdateStatus
    {
        LauncherUpdateCheck = 0,
        ClientUpdateCheck = 1,
        ClientUpdate = 2,
        ClientUpdateExtracting = 3,
        Ready = 4,
        Error = 5,
        LauncherNeedsUpdate = 6,
    }
}