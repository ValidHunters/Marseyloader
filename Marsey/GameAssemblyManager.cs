using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HarmonyLib;
using Marsey.Stealthsey;

namespace Marsey;

public static class HarmonyManager
{
    private static Harmony? _harmony;

    /// <summary>
    /// Sets the _harmony field in the class.
    ///
    /// If debug is enabled - log IL to file on desktop
    /// </summary>
    /// <param name="harmony">A Harmony instance</param>
    public static void Init(Harmony harmony)
    {
        _harmony = harmony;

        if (MarseyVars.DebugAllowed)
            Harmony.DEBUG = true;
    }

    public static Harmony? GetHarmony() => _harmony;
}

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

public static class GameAssemblyManager
{
    private static readonly Dictionary<string, Assembly?> _assemblies = new Dictionary<string, Assembly?>
    {
        { "Content.Client,", null },
        { "Robust.Shared,", null },
        { "Content.Shared,", null }
    };

    /// <summary>
    /// Retrieves game assemblies and logs if any are missing.
    /// </summary>
    public static void GetGameAssemblies(out Assembly? clientAss, out Assembly? robustSharedAss, out Assembly? clientSharedAss)
    {
        if (!TryGetAssemblies())
            LogMissingAssemblies();

        clientAss = _assemblies["Content.Client,"];
        robustSharedAss = _assemblies["Robust.Shared,"];
        clientSharedAss = _assemblies["Content.Shared,"];

        MarseyLogger.Log(MarseyLogger.LogType.INFO, "Received assemblies.");
    }

    private static bool TryGetAssemblies()
    {
        for (int loops = 0; loops < MarseyVars.MaxLoops; loops++)
        {
            if (FindAssemblies())
                return true;

            Thread.Sleep(MarseyVars.LoopCooldown);
        }
        return false;
    }

    private static bool FindAssemblies()
    {
        Assembly[] asmlist = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly asm in asmlist)
        {
            string? fullName = asm.FullName;
            if (fullName == null) continue;

            foreach (string entry in _assemblies.Keys.ToList())
            {
                AssignAssembly(entry, asm);
            }

            if (_assemblies.Values.All(a => a != null))
                return true;
        }
        return false;
    }

    private static void AssignAssembly(string assemblyName, Assembly asm)
    {
        if (_assemblies[assemblyName] == null && asm.FullName?.Contains(assemblyName) == true)
            _assemblies[assemblyName] = asm;
    }

    private static void LogMissingAssemblies()
    {
        foreach (KeyValuePair<string, Assembly?> entry in _assemblies)
        {
            if (entry.Value == null)
                MarseyLogger.Log(MarseyLogger.LogType.WARN, $"{entry.Key} assembly was not received.");
        }
    }
}

