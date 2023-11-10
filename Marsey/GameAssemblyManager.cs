using System;
using System.Reflection;
using System.Threading;
using HarmonyLib;

namespace Marsey;

/// <summary>
/// Manages game's assemblies, this includes patching.
/// </summary>
public abstract class GameAssemblyManager
{
    private static Harmony? _harmony;

    /// <summary>
    /// Sets the _harmony field in the class.
    /// </summary>
    /// <param name="harmony">A Harmony instance</param>
    public static void Init(Harmony harmony) => _harmony = harmony;

    /// <summary>
    /// Patches the game using assemblies in List.
    /// </summary>
    public static void PatchProc()
    {
        if (_harmony == null) return;

        foreach (var patch in PatchAssemblyManager.GetPatchList())
        {
            AssemblyName assemblyName = patch.Asm.GetName();
            Utility.Log(Utility.LogType.INFO, $"Patching {assemblyName}");

            try
            {
                _harmony.PatchAll(patch.Asm);
            }
            catch (Exception e)
            {
                string errorMessage = $"Failed to patch {assemblyName.Name}!\n{e}";

                if (MarseyVars.ThrowOnFail)
                    throw new PatchAssemblyException(errorMessage);

                Utility.Log(Utility.LogType.FATL, errorMessage);
            }
        }
    }

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
    public static void GetGameAssemblies(out Assembly? clientAss, out Assembly? robustSharedAss, out Assembly? clientSharedAss)
    {
        clientAss = null;
        robustSharedAss = null;
        clientSharedAss = null;

        int loops;
        for (loops = 0; loops < MarseyVars.MaxLoops; loops++)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
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
                Utility.Log(Utility.LogType.INFO, "Received assemblies.");
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
            Utility.Log(Utility.LogType.WARN, "Content.Client assembly was not received.");
        if (clientSharedAss == null)
            Utility.Log(Utility.LogType.WARN, "Client.Shared assembly was not received.");
        if (robustSharedAss == null)
            Utility.Log(Utility.LogType.WARN, "Robust.Shared assembly was not received");
    }
}
