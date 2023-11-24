using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Marsey.Subversion;


public static class Subverter
{
    private static List<MarseyPatch> _subverterPatches = new List<MarseyPatch>();

    public static void LoadSubverts()
    {
        string[] path = new string[] { "Subversion" };
        FileHandler.LoadAssemblies(path, subverter: true);
    }

    public static void AddSubvert(MarseyPatch patch)
    {
        Assembly assembly = patch.Asm;

        if (_subverterPatches.Any(p => p.Asm == assembly)) return;

        _subverterPatches.Add(patch);
    }

    public static void SubversionPatcher()
    {

    }

    public static List<MarseyPatch> GetSubverterPatches() => _subverterPatches;
}
