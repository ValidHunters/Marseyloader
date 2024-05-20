using System.Collections.Generic;
using System.IO;
using Marsey.Config;

namespace SS14.Launcher.Marseyverse;

public static class Persist
{
    public static void SaveConfig(List<string> patches)
    {
        File.WriteAllLines(Path.Combine(LauncherPaths.DirUserData, MarseyVars.EnabledPatchListFileName), patches);
    }

    public static List<string> LoadConfig()
    {
        string filePath = Path.Combine(LauncherPaths.DirUserData, MarseyVars.EnabledPatchListFileName);
        return File.Exists(filePath) ? [..File.ReadAllLines(filePath)] : [];
    }
}
