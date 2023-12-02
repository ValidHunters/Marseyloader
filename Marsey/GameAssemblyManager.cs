using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using HarmonyLib;

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
            AssemblyName assemblyName = patch.Asm.GetName();
            MarseyLogger.Log(MarseyLogger.LogType.INFO, $"Patching {assemblyName}");

            try
            {
                harmony.PatchAll(patch.Asm);
            }
            catch (Exception e)
            {
                string errorMessage = $"Failed to patch {assemblyName.Name}!\n{e}";

                if (MarseyVars.ThrowOnFail)
                    throw new PatchAssemblyException(errorMessage);

                MarseyLogger.Log(MarseyLogger.LogType.FATL, errorMessage);
            }
        }
    }

}

public abstract class GameAssemblyManager
{
    /// <summary>
    /// Obtains game assemblies.
    /// The function ends only when Robust.Shared,
    /// Content.Client and Content.Shared are initialized by the game,
    /// or MarseyVars.MaxLoops loops have been made without obtaining all the assemblies.
    ///
    /// Executed only by the Loader.
    /// </summary>
    /// <exception cref="Exception">Excepts if manager couldn't get game assemblies after $MaxLoops loops.</exception>
    /// <see cref="MarseyVars.MaxLoops"/>
    /// <remarks>Subversion patches skip this entirely and are loaded before game assemblies are obtained by the engine</remarks>
    public static void GetGameAssemblies(out Assembly? clientAss, out Assembly? robustSharedAss, out Assembly? clientSharedAss)
    {
        clientAss = null;
        robustSharedAss = null;
        clientSharedAss = null;

        int loops;
        for (loops = 0; loops < MarseyVars.MaxLoops; loops++)
        {
            Assembly[] asmlist = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly asm in asmlist)
            {
                string? fullName = asm.FullName;
                if (fullName == null) continue;

                if (robustSharedAss == null && fullName.Contains("Robust.Shared,"))
                    robustSharedAss = asm;
                else if (clientAss == null && fullName.Contains("Content.Client,"))
                    clientAss = asm;
                else if (clientSharedAss == null && fullName.Contains("Content.Shared,"))
                    clientSharedAss = asm;
                
                if (robustSharedAss != null && clientAss != null && clientSharedAss != null)
                    break;
            }

            if (robustSharedAss != null && clientAss != null && clientSharedAss != null)
            {
                MarseyLogger.Log(MarseyLogger.LogType.INFO, "Received assemblies.");
                break;
            }

            Thread.Sleep(MarseyVars.LoopCooldown);
        }

        if (loops >= MarseyVars.MaxLoops)
            LogMissingAssemblies(clientAss, robustSharedAss, clientSharedAss);
    }

    /// <summary>
    /// Warn that assemblies could not be received.
    /// Executed only when MarseyVars.MaxLoops was exhausted.
    /// </summary>
    /// <see cref="MarseyVars.MaxLoops"/>
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
