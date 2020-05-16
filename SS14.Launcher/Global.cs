using System.Net.Http;

namespace SS14.Launcher
{
    public static class Global
    {
        public const ushort DefaultServerPort = 1212;

        /// <summary>
        ///     Global HTTP client with correct User-Agent set.
        /// </summary>
        public static HttpClient GlobalHttpClient { get; }

        static Global()
        {
            var version = typeof(Global).Assembly.GetName().Version;
            GlobalHttpClient = new HttpClient();
            GlobalHttpClient.DefaultRequestHeaders.Add("User-Agent", $"SS14.Launcher v{version}");
        }
    }
}