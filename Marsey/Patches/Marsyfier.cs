using Marsey.Game;
using Marsey.Misc;
using Marsey.PatchAssembly;

namespace Marsey.Patches;

/// <summary>
/// Manages MarseyPatch instances
/// </summary>
public static class Marsyfier
{
    public static List<MarseyPatch> GetMarseyPatches() => PatchListManager.GetPatchList<MarseyPatch>();

    /// <summary>
    /// Start preload of marseypatches that are flagged as such
    /// </summary>
    public static void Preload(string[]? path = null)
    {
        List<string> preloads = FileHandler.GetFilesFromPipe("PreloadMarseyPatchesPipe");

        if (preloads.Count == 0) return;

        MarseyLogger.Log(MarseyLogger.LogType.INFO, "Preloader", $"Preloading {preloads.Count} patches.");

        foreach (string patch in preloads)
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Preloader", $"Preloading {patch}");
            FileHandler.LoadExactAssembly(patch);
        }

        List<MarseyPatch> preloadedPatches = GetMarseyPatches();

        AssemblyFieldHandler.InitHelpers(preloadedPatches);

        if (preloadedPatches.Count != 0) Patcher.Patch(preloadedPatches);

        PatchListManager.ResetList();
    }
}
