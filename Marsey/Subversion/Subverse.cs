using System.Collections.Generic;
using System.Linq;

namespace Marsey.Subversion;

/// <summary>
/// Manages the Subverter helper patch
/// https://github.com/Subversionary/Subverter
/// </summary>
public static class Subverse
{
    private static MarseyPatch? _subverter = null; 
    
    /// <summary>
    /// Initializes the subverter library and subversion patches
    /// </summary>
    /// <returns>True if the library was initialized successfully</returns>
    public static bool InitSubverter()
    {
        if (_subverter != null)
            return true;
        
        Subverter.LoadSubverts();
        List<MarseyPatch> patches = Subverter.GetSubverterPatches();
        foreach (MarseyPatch p in patches)
        {
            if (p.Name == "Subverter")
            {
                AssignSubverter(p);
                PatchAssemblyManager.InitLogger(patches);
                patches.RemoveAll(p => p.Name == "Subverter");
                return true;
            }
        }

        return false;
    }

    private static void AssignSubverter(MarseyPatch subverter)
    {
        _subverter = subverter;
    }
    
    /// <summary>
    /// Enables subverter if any of the of the subverter patches are enabled
    /// </summary>
    public static void CheckEnabled()
    {
        List<MarseyPatch> patches = Subverter.GetSubverterPatches();

        if (patches.Any(p => p.Enabled))
        {
            MarseyVars.Subverter = true;
            return;
        }

        MarseyVars.Subverter = false;
    }

    /// <summary>
    /// Patches subverter ahead of everything else
    /// This is done as we attach to the assembly loading function
    /// </summary>
    public static void PatchSubverter()
    {
        if (_subverter != null) GameAssemblyManager.PatchProc(new List<MarseyPatch>() { _subverter });
    }
}