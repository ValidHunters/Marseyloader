using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SS14.Launcher.Models
{
    // Fallback path of the updater pulling a game version off the builds server.
    public sealed partial class Updater
    {
        public const string JenkinsBaseUrl = "https://builds.spacestation14.io/jenkins";
        private static readonly string JenkinsJobName = Uri.EscapeUriString("SS14 Content");

        private static async Task<ServerBuildInformation> GetDefaultBuildInformation()
        {
            var jobUri = new Uri($"{JenkinsBaseUrl}/job/{JenkinsJobName}/api/json");
            var jobDataResponse = await Global.GlobalHttpClient.GetAsync(jobUri);
            if (!jobDataResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Got bad status code {jobDataResponse.StatusCode} from Jenkins.");
            }

            var jobInfo =
                JsonConvert.DeserializeObject<JenkinsJobInfo>(await jobDataResponse.Content.ReadAsStringAsync());

            var latestBuildNumber = jobInfo!.LastSuccessfulBuild!.Number;

            string PlatformUrl(string platform)
            {
                return
                    $"{JenkinsBaseUrl}/job/{JenkinsJobName}/{latestBuildNumber}/artifact/release/{Uri.EscapeUriString(GetBuildFilename(platform))}";
            }

            return new ServerBuildInformation
            {
                DownloadUrls = new PlatformList
                {
                    Windows = PlatformUrl("Windows"),
                    Linux = PlatformUrl("Linux"),
                    MacOS = PlatformUrl("macOS"),
                },
                ForkId = "__default",
                Version = latestBuildNumber.ToString(CultureInfo.InvariantCulture),
                Hashes = new PlatformList()
            };
        }

        [Pure]
        private static string GetBuildFilename(string platform)
        {
            return $"SS14.Client_{platform}_x64.zip";
        }
    }
}
