using System;
using Marsey.Config;

namespace SS14.Launcher;

public static class LauncherVersion
{
    public const string Name = "Marseyloader";
    public static Version? Version => typeof(LauncherVersion).Assembly.GetName().Version;
}
