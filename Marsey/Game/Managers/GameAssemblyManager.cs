using System.Reflection;
using Marsey.Config;
using Marsey.Game.Misc;
using Marsey.Misc;

namespace Marsey.Game.Managers;

public static class GameAssemblyManager
{
    private static readonly Dictionary<string, Assembly?> _assemblies = new Dictionary<string, Assembly?>
    {
        { "Content.Client,", null },
        { "Content.Shared,", null }
    };

    public static Assembly? GetSharedEngineAssembly()
    {
        Assembly? RobustShared = FindAssembly("Robust.Shared,");
        if (RobustShared == null) MarseyLogger.Log(MarseyLogger.LogType.WARN, "Failed to find Shared engine assembly!");

        return RobustShared;
    }

    /// <summary>
    /// Retrieves game assemblies and logs if any are missing.
    /// </summary>
    public static void TrySetContentAssemblies()
    {
        if (!TryGetAssemblies())
            LogMissingAssemblies();

        GameAssemblies.AssignContentAssemblies(_assemblies["Content.Client,"], _assemblies["Content.Shared,"]);

        MarseyLogger.Log(MarseyLogger.LogType.INFO, "Received assemblies.");
    }

    /// <summary>
    /// Searches for assemblies defined in the _assemblies dictionary.
    /// </summary>
    /// <returns>True if all assemblies in dictionary have been found, false otherwise</returns>
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
    
    /// <inheritdoc cref="TryGetAssemblies"/>
    private static bool FindAssemblies()
    {
        foreach (string entry in _assemblies.Keys.ToList())
        {
            Assembly? foundAssembly = FindAssembly(entry);
            if (foundAssembly != null)
            {
                AssignAssembly(entry, foundAssembly);
            }
        }
    
        return _assemblies.Values.All(a => a != null);
    }
    
    private static Assembly? FindAssembly(string assemblyName)
    {
        Assembly[] asmlist = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly asm in asmlist)
        {
            if (asm.FullName?.Contains(assemblyName) == true)
                return asm;
        }
        return null;
    }

    /// <summary>
    /// Assigns an assembly to the dictionary if it matches the provided name and is not already assigned.
    /// </summary>
    private static void AssignAssembly(string assemblyName, Assembly asm)
    {
        if (_assemblies[assemblyName] == null && asm.FullName?.Contains(assemblyName) == true)
            _assemblies[assemblyName] = asm;
    }

    /// <summary>
    /// If any assemblies are missing - notify.
    /// </summary>
    private static void LogMissingAssemblies()
    {
        foreach (KeyValuePair<string, Assembly?> entry in _assemblies)
        {
            if (entry.Value == null)
                MarseyLogger.Log(MarseyLogger.LogType.WARN, $"{entry.Key} assembly was not received.");
        }
    }
}

