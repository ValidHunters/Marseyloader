using System.Reflection;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Marsey.Config;
using Marsey.GameAssembly;
using Marsey.PatchAssembly;
using Marsey.Serializer;
using Marsey.Stealthsey;
using Marsey.Utility;

namespace Marsey.Patches;

/// <summary>
/// Manages MarseyPatch instances
/// </summary>
public static class Marsyfier
{
    public const string MarserializerFile = "patches.marsey";
    public const string PreloadMarserializerFile = "preload.marsey";

    public static List<MarseyPatch> GetMarseyPatches()
    {
        return PatchListManager.GetPatchList<MarseyPatch>();
    }

    public static void Preload(string[]? path = null)
    {
        if (MarseyVars.MarseyHide >= HideLevel.Unconditional) return;
        
        path ??= new[] { MarseyVars.MarseyPatchFolder };

        List<string>? preloads = Marserializer.Deserialize(path, filename: PreloadMarserializerFile);

        if (preloads == null || preloads.Count == 0) return;
        
        MarseyLogger.Log(MarseyLogger.LogType.INFO, $"Preloading {preloads.Count} patches");

        foreach (string patch in preloads)
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"Preloading {patch}");
            FileHandler.LoadExactAssembly(patch);
        }

        List<MarseyPatch> preloadedPatches = GetMarseyPatches();

        if (preloadedPatches.Count != 0) GamePatcher.Patch(preloadedPatches);
        
        PatchListManager.ResetList();
    }
}

/// <summary>
/// The bread and butter of the loader, the MarseyPatch.
/// This class contains the data about a patch (called a Marsey), that is later used the loader to alter the game's functionality.
/// </summary>
public class MarseyPatch : IPatch
{
    public string Asmpath { get; set; } // DLL file path
    public Assembly Asm { get; set; } // Assembly containing the patch
    public string Name { get; set; } // Patch's name
    public string Desc { get; set; } // Patch's description
    public bool Preload { get; set; } = false; // Is the patch getting loaded before game assemblies
    public bool Enabled { get; set; } // Is the patch enabled or not.

    public MarseyPatch(string asmpath, Assembly asm, string name, string desc, bool preload = false)
    {
        this.Asmpath = asmpath;
        this.Name = name;
        this.Desc = desc;
        this.Asm = asm;
        this.Preload = preload;
    }
}