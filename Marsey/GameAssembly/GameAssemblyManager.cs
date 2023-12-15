using System;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Marsey.Config;
using Marsey.Utility;

namespace Marsey.GameAssembly;

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

