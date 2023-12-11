using System;
using System.Reflection;
using System.Collections.Generic;
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
    public static void GetGameAssemblies(out Assembly? clientAss, out Assembly? robustSharedAss, out Assembly? clientSharedAss)
    {
        InitializeAssemblyVariables(out clientAss, out robustSharedAss, out clientSharedAss);

        if (!TryGetAssemblies(ref clientAss, ref robustSharedAss, ref clientSharedAss))
            LogMissingAssemblies(clientAss, robustSharedAss, clientSharedAss);

        MarseyLogger.Log(MarseyLogger.LogType.INFO, "Received assemblies.");
    }

    private static void InitializeAssemblyVariables(out Assembly? clientAss, out Assembly? robustSharedAss, out Assembly? clientSharedAss)
    {
        clientAss = null;
        robustSharedAss = null;
        clientSharedAss = null;
    }

    private static bool TryGetAssemblies(ref Assembly? clientAss, ref Assembly? robustSharedAss, ref Assembly? clientSharedAss)
    {
        for (int loops = 0; loops < MarseyVars.MaxLoops; loops++)
        {
            if (FindAssemblies(ref clientAss, ref robustSharedAss, ref clientSharedAss))
                return true;

            Thread.Sleep(MarseyVars.LoopCooldown);
        }
        return false;
    }

    private static bool FindAssemblies(ref Assembly? clientAss, ref Assembly? robustSharedAss, ref Assembly? clientSharedAss)
    {
        Assembly[] asmlist = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly asm in asmlist)
        {
            string? fullName = asm.FullName;
            if (fullName == null) continue;

            AssignAssembly(ref clientAss, "Content.Client,", asm);
            AssignAssembly(ref robustSharedAss, "Robust.Shared,", asm);
            AssignAssembly(ref clientSharedAss, "Content.Shared,", asm);

            if (clientAss != null && robustSharedAss != null && clientSharedAss != null)
                return true;
        }
        return false;
    }

    private static void AssignAssembly(ref Assembly? targetAssembly, string assemblyName, Assembly asm)
    {
        if (targetAssembly == null && asm.FullName?.Contains(assemblyName) == true)
            targetAssembly = asm;
    }

    private static void LogMissingAssemblies(Assembly? clientAss, Assembly? robustSharedAss, Assembly? clientSharedAss)
    {
        if (clientAss == null)
            MarseyLogger.Log(MarseyLogger.LogType.WARN, "Content.Client assembly was not received.");
        if (clientSharedAss == null)
            MarseyLogger.Log(MarseyLogger.LogType.WARN, "Client.Shared assembly was not received.");
        if (robustSharedAss == null)
            MarseyLogger.Log(MarseyLogger.LogType.WARN, "Robust.Shared assembly was not received");
    }
}

