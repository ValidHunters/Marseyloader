using System.Collections.Generic;
using System.IO;
using Marsey.Config;
using Splat;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Utility;

namespace SS14.Launcher.Marseyverse;

public static class Persist
{
    public static void SavePatchlistConfig(List<string> patches)
    {
        File.WriteAllLines(Path.Combine(LauncherPaths.DirUserData, MarseyVars.EnabledPatchListFileName), patches);
    }

    public static List<string> LoadPatchlistConfig()
    {
        string filePath = Path.Combine(LauncherPaths.DirUserData, MarseyVars.EnabledPatchListFileName);
        return File.Exists(filePath) ? [..File.ReadAllLines(filePath)] : [];
    }

    public static void UpdateLauncherConfig()
    {
        DataManager cfg = Locator.Current.GetRequiredService<DataManager>();

        MarseyConf.Logging = cfg.GetCVar(CVars.LogLauncherPatcher);
        MarseyConf.DebugAllowed = cfg.GetCVar(CVars.LogLoaderDebug);
        MarseyConf.TraceAllowed = cfg.GetCVar(CVars.LogLoaderTrace);
    }
}
