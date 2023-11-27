using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Marsey.Subversion;


/// <summary>
/// Manages patches/addons based on the Subverter patch
/// </summary>
public static class Subverter
{
    private static List<MarseyPatch> _subverterPatches = new List<MarseyPatch>();
    
    public static void LoadSubverts()
    {
        string[] path = { MarseyVars.SubverterPatchFolder };
        FileHandler.LoadAssemblies(path, subverter: true);
    }

    public static void AddSubvert(MarseyPatch patch)
    {
        string assemblypath = patch.Asmpath;
        
        if (Subverse.CheckSubverterDuplicate(patch)) return;

        if (_subverterPatches.Any(p => p.Asmpath == assemblypath)) return;

        _subverterPatches.Add(patch);
    }

    public static List<MarseyPatch> GetSubverterPatches() => _subverterPatches;
}
