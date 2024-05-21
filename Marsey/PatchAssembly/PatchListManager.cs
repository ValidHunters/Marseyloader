using Marsey.Patches;
using System.Collections.Generic;
using System.Linq;
using Marsey.Config;
using Marsey.Misc;

namespace Marsey.PatchAssembly;

/// <summary>
/// Manages patch lists.
/// </summary>
public static class PatchListManager
{
    private static readonly List<IPatch> _patches = new List<IPatch>();

    /// <summary>
    /// Checks if the amount of patches in folder equals the amount of patches in list.
    /// If not - resets the lists.
    /// </summary>
    public static void RecheckPatches()
    {
        int folderPatchCount = FileHandler.GetPatches(new[] { MarseyVars.MarseyPatchFolder }).Count;
        if (folderPatchCount != _patches.Count)
        {
            ResetList();
        }
    }

    /// <summary>
    /// Adds a patch to the list if it is not already present.
    /// </summary>
    /// <param name="patch">The patch to add.</param>
    public static void AddPatchToList(IPatch patch)
    {
        if (_patches.Any(p => p.Asmpath == patch.Asmpath)) return;

        MarseyLogger.Log(MarseyLogger.LogType.TRCE, $"Adding {patch.Name} ({patch.Asmpath}) to patchlist");
        _patches.Add(patch);
    }

    /// <summary>
    /// Returns the list of patches of a specific type.
    /// </summary>
    public static List<T> GetPatchList<T>() where T : IPatch
    {
        return _patches.OfType<T>().ToList();
    }

    /// <summary>
    /// Clears the list of patches.
    /// </summary>
    public static void ResetList()
    {
        _patches.Clear();
    }
}
