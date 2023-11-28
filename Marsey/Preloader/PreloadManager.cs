using System.Collections.Generic;
using Marsey.Serializer;

namespace Marsey.Preloader;

/// <summary>
/// Manages preloading marsey patches
/// </summary>
public static class PreloadManager
{
    public const string MarserializerFile = "preload.marsey";

    public static void Preload(string[]? path = null)
    {
        path ??= new[] { MarseyVars.MarseyPatchFolder };
        
        List<string>? preloads = Marserializer.Deserialize(path, filename: MarserializerFile);

        if (preloads == null) return;
        
        MarseyLogger.Log(MarseyLogger.LogType.INFO, $"Preloading {preloads.Count} patches");

        foreach (string patch in preloads)
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"Preloading {patch}");
            FileHandler.LoadExactAssembly(patch);
        }

        List<MarseyPatch>? preloadedPatches = PatchListManager.GetPatchList<MarseyPatch>();

        if (preloadedPatches != null) GamePatcher.Patch(preloadedPatches);
        
        PatchListManager.ResetList();
    }
}