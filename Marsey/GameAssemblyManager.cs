using System;
using System.Reflection;
using System.Threading;
using HarmonyLib;

namespace Marsey;

/// <summary>
/// Manages game's assemblies, this includes patching.
/// </summary>
public class GameAssemblyManager
{
    private static Harmony? _harmony;

    /// <summary>
    /// Sets the _harmony field in the class.
    /// </summary>
    /// <param name="harmony">A Harmony instance</param>
    public static void Init(Harmony harmony)
    {
        _harmony = harmony;
    }

    /// <summary>
    /// Patches the game using assemblies in List.
    /// </summary>
    public static void PatchProc()
    {
        if (_harmony != null)
        {
            foreach (MarseyPatch p in PatchAssemblyManager.GetPatchList())
            {
                Console.WriteLine($"[MARSEY] Patching {p.Asm.GetName()}");
                _harmony.PatchAll(p.Asm);
            }
        }
    }

    /// <summary>
    /// Obtains game assemblies.
    /// The function ends only when Robust.Shared,
    /// Content.Client and Content.Shared are initialized by the game,
    /// or 100 loops have been made without obtaining all the assemblies.
    ///
    /// Executed only by the Loader.
    /// </summary>
    /// <exception cref="Exception">Excepts if manager couldn't get game assemblies after 100 loops.</exception>
    public static void GetGameAssemblies(out Assembly? clientAss, out Assembly? robustSharedAss, out Assembly? clientSharedAss)
    {
        clientAss = null;
        robustSharedAss = null;
        clientSharedAss = null;

        int loops = 0;
        while (robustSharedAss == null || clientAss == null || clientSharedAss == null && loops < 100)
        {
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var e in asms)
            {
                string? fullName = e.FullName;
                if (fullName != null)
                {
                    if (robustSharedAss == null && fullName.Contains("Robust.Shared,"))
                    {
                        robustSharedAss = e;
                    }
                    else if (clientAss == null && fullName.Contains("Content.Client,"))
                    {
                        clientAss = e;
                    }
                    else if (clientSharedAss == null && fullName.Contains("Content.Shared,"))
                    {
                        clientSharedAss = e;
                    }
                }
            }

            loops++;
            Thread.Sleep(200);
        }

        if (loops >= 100)
            throw new Exception("Failed to receive assemblies within 100 loops.");
        else
            Console.WriteLine("[MARSEY] Received assemblies.");
    }
}
