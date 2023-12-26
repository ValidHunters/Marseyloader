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
    private static List<string>? _subverters = null;
    
    public static bool CheckSubversions()
    {
        _subverters = Marserializer.Deserialize(new string[]{MarseyVars.MarseyPatchFolder}, Subverter.MarserializerFile);

        return _subverters != null && _subverters.Count != 0;
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
    
        foreach (string path in _subverters!)
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
    
    private static MethodInfo? CheckEntry(Assembly assembly)
    {
        Type? entryType = assembly.GetType("MarseyEntry");
        if (entryType == null) return null;
        
        MethodInfo? entryMethod = AssemblyFieldHandler.GetEntry(assembly, entryType);
        return entryMethod != null ? entryMethod : null;
    }
}