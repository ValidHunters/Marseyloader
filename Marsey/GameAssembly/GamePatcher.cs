using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Marsey.Config;
using Marsey.Patches;
using Marsey.Utility;

namespace Marsey.GameAssembly;

public static class GamePatcher
{
    /// <summary>
    /// Patches the game using assemblies in List.
    /// </summary>
    /// <param name="patchlist">A list of patches</param>
    public static void Patch<T>(List<T> patchlist) where T : IPatch
    {
        Harmony? harmony = HarmonyManager.GetHarmony();
        if (harmony == null) return;

        foreach (T patch in patchlist)
        {
            PatchAssembly(harmony, patch);
        }
    }

    private static void PatchAssembly<T>(Harmony harmony, T patch) where T : IPatch
    {
        AssemblyName assemblyName = patch.Asm.GetName();
        MarseyLogger.Log(MarseyLogger.LogType.INFO, $"Patching {assemblyName}");

        try
        {
            harmony.PatchAll(patch.Asm);
        }
        catch (Exception e)
        {
            HandlePatchException(assemblyName.Name!, e);
        }
    }

    private static void HandlePatchException(string assemblyName, Exception e)
    {
        string errorMessage = $"Failed to patch {assemblyName}!\n{e}";

        if (MarseyVars.ThrowOnFail)
            throw new PatchAssemblyException(errorMessage);

        MarseyLogger.Log(MarseyLogger.LogType.FATL, errorMessage);
    }
}