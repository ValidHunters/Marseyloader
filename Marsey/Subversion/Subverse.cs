using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Marsey.Config;
using Marsey.GameAssembly;
using Marsey.Handbrake;
using Marsey.Stealthsey;
using Marsey.Misc;
using Marsey.PatchAssembly;
using Marsey.Serializer;
using Marsey.Stealthsey.Reflection;

namespace Marsey.Subversion;

/// <summary>
/// Manages the Subverter helper patch
/// https://github.com/Subversionary/Subverter
/// </summary>
public static class Subverse
{
    /// <summary>
    /// Enables subverter if any of the of the subverter patches are enabled
    /// Used by the launcher to determine if it should load subversions
    /// </summary>
    /// <remarks>If MarseyHide is set to unconditional - defaults to false</remarks>
    public static void CheckEnabled()
    {
        List<SubverterPatch> patches = Subverter.GetSubverterPatches();

        if (patches.Any(p => p.Enabled))
        {
            MarseyVars.Subverter = true;
            return;
        }

        MarseyVars.Subverter = false;
    }
    
    /// <summary>
    /// Patches subverter ahead of everything else
    /// This is done as we attach to the assembly loading function
    /// </summary>
    public static void PatchSubverter()
    {
        MethodInfo Target = AccessTools.Method(AccessTools.TypeByName("Robust.Shared.ContentPack.ModLoader"), "TryLoadModules");
        MethodInfo Prefix = typeof(Subverse).GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Static)!;
        
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Subversion", $"Hooking {Target.Name} with {Prefix.Name}");
        
        Manual.Patch(Target, Prefix, HarmonyPatchType.Prefix);
    }

    private static bool Prefix(object __instance)
    {
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Subversion", "Detour");
        MethodInfo? loadGameAssemblyMethod = AccessTools.Method(AccessTools.TypeByName("Robust.Shared.ContentPack.BaseModLoader"), "InitMod");
    
        foreach (string path in GetSubverters())
        {
            Assembly subverterAssembly = Assembly.LoadFrom(path);
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Subversion", $"Sideloading {path}");
            loadGameAssemblyMethod.Invoke(__instance, new object[] { subverterAssembly });
            
            MethodInfo? Entry = CheckEntry(subverterAssembly);
            if (Entry != null)
                Doorbreak.Enter(Entry);
            
            Hidesey.HidePatch(subverterAssembly);
        }
        
        return true;
    }
    
    private static IEnumerable<string> GetSubverters()
    {
        string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Marsey");
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Subversion", $"Loading from {directoryPath}");
        
        List<string> patches = Marserializer.Deserialize(new string[] { directoryPath }, Subverter.MarserializerFile) ?? new List<string>();

        foreach (string filePath in patches)
        {
            yield return filePath;
        }
    }

    private static MethodInfo? CheckEntry(Assembly assembly)
    {
        Type? entryType = assembly.GetType("MarseyEntry");
        if (entryType == null) return null;
        
        MethodInfo? entryMethod = AssemblyFieldHandler.GetEntry(assembly, entryType);
        return entryMethod != null ? entryMethod : null;
    }
}