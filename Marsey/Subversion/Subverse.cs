using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;

namespace Marsey.Subversion;

/// <summary>
/// Manages the Subverter helper patch
/// https://github.com/Subversionary/Subverter
/// </summary>
public static class Subverse
{
    private static SubverterPatch? _subverter = null;

    /// <summary>
    /// Initializes the subverter library
    /// </summary>
    /// <returns>True if the library was initialized successfully</returns>
    public static bool InitSubverter()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), MarseyVars.SubverterPatchFolder, "Subverter.dll");
        FileHandler.LoadExactAssembly(path);
        
        List<SubverterPatch> patches = Subverter.GetSubverterPatches();
        foreach (SubverterPatch p in patches.Where(p => p.Name == "Subverter"))
        {
            AssignSubverter(p);
            patches.Clear();
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Enables subverter if any of the of the subverter patches are enabled
    /// </summary>
    public static void CheckEnabled()
    {
        List<SubverterPatch> patches = Subverter.GetSubverterPatches();

        if (patches.Any(p => p.Enabled))
        {
            MarseyVars.Subverter = true;
            return;
        }

        MarseyVars.Subverter = false;
    }

    /// <summary>
    /// Check if a patch is loaded from the same place subverter is
    /// </summary>
    /// <returns></returns>
    public static bool CheckSubverterDuplicate(SubverterPatch subverter)
    {
        return subverter.Asmpath == _subverter?.Asmpath;
    }

    public static bool CheckSubverterPresent()
    {
        return _subverter != null;
    }
    
    /// <summary>
    /// Patches subverter ahead of everything else
    /// This is done as we attach to the assembly loading function
    /// </summary>
    public static void PatchSubverter()
    {
        if (_subverter != null) GamePatcher.Patch(new List<SubverterPatch>() { _subverter });
    }
    
    
    private static void AssignSubverter(SubverterPatch subverter)
    {
        _subverter = subverter;
    }
}