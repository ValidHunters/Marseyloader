using System;

namespace SS14.Launcher
{
    public static class ConfigConstants
    {
        public const string CurrentLauncherVersion = "5";

        // Refresh login tokens if they're within <this much> of expiry.
        public static readonly TimeSpan TokenRefreshThreshold = TimeSpan.FromDays(15);

        public const string HubUrl = "https://builds.spacestation14.io/hub/";
        public const string AuthUrl = "http://localhost:5000/";
        public const string DiscordUrl = "https://discord.gg/t2jac3p";
        public const string WebsiteUrl = "https://spacestation14.io";
        public const string DownloadUrl = "https://spacestation14.io/about/nightlies/";
    }
}
