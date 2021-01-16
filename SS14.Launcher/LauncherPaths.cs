using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SS14.Launcher
{
    /// <summary>
    ///     Contains file paths used by the launcher to manage data.
    /// </summary>
    public static class LauncherPaths
    {
        public static readonly string AppDataPath = Path.Combine("Space Station 14", "launcher");
        public static readonly string EngineInstallationsDirName = "engines";
        public static readonly string ServerContentDirName = "server content";
        public static readonly string LogsDirName = "logs";
        public static readonly string LauncherLogName = "launcher.log";
        public static readonly string ClientMacLogName = "client.mac.log";
        public static readonly string ClientStdoutLogName = "client.stdout.log";
        public static readonly string ClientStderrLogName = "client.stderr.log";

        public static readonly string DirLauncherInstall = GetInstallDir();
        public static readonly string DirUserData = GetUserDataDir();
        public static readonly string DirEngineInstallations = Path.Combine(DirUserData, EngineInstallationsDirName);
        public static readonly string DirServerContent = Path.Combine(DirUserData, ServerContentDirName);
        public static readonly string DirLogs = Path.Combine(DirUserData, LogsDirName);
        public static readonly string PathLauncherLog = Path.Combine(DirLogs, LauncherLogName);
        public static readonly string PathClientMacLog = Path.Combine(DirLogs, ClientMacLogName);
        public static readonly string PathClientStdoutLog = Path.Combine(DirLogs, ClientStdoutLogName);
        public static readonly string PathClientStderrLog = Path.Combine(DirLogs, ClientStderrLogName);

        public static void CreateDirs()
        {
            if (!Directory.Exists(DirLogs))
                Directory.CreateDirectory(DirLogs);

            if (!Directory.Exists(DirServerContent))
                Directory.CreateDirectory(DirServerContent);

            if (!Directory.Exists(DirEngineInstallations))
                Directory.CreateDirectory(DirEngineInstallations);
        }

        private static string GetInstallDir()
        {
            return Path.GetDirectoryName(typeof(LauncherPaths).Assembly.Location)!;
        }

        private static string GetUserDataDir()
        {
            string appDataDir;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                if (xdgDataHome == null)
                {
                    appDataDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");
                }
                else
                {
                    appDataDir = xdgDataHome;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Library", "Application Support");
            }
            else
            {
                appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }

            return Path.Combine(appDataDir, AppDataPath);
        }
    }
}
