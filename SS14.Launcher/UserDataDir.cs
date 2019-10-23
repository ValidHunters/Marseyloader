using System;
using System.IO;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace SS14.Launcher
{
    public static class UserDataDir
    {
        [Pure]
        public static string GetUserDataDir()
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

            return Path.Combine(appDataDir, "Space Station 14", "launcher");
        }
    }
}