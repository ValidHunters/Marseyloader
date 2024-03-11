using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Marsey.Config;
using Marsey.Game;
using Marsey.Game.Misc;
using Marsey.Handbreak;
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

    /// <summary>
    /// Check if we have any subversions enabled
    /// </summary>
    public static bool CheckSubversions()
    {
        _subverters =
            Marserializer.Deserialize(new string[] { MarseyVars.MarseyPatchFolder }, Subverter.MarserializerFile);

        return _subverters != null && _subverters.Count != 0;
    }

    /// <summary>
    /// Patches subverter ahead of everything else
    /// This is done as we attach to the assembly loading function
    /// </summary>
    public static void PatchSubverter()
    {
        MethodInfo? Target = Helpers.GetMethod("Robust.Shared.ContentPack.ModLoader", "TryLoadModules");
        MethodInfo? PatchMethod;
        if (!MarseyConf.SubvertPreload)
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Subversion", "Start postfix patch of TryLoadModules");
            PatchMethod = Helpers.GetMethod(typeof(Subverse), "Postfix");
        }
        else
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Subversion", "Start prefix patch of TryLoadModules");
            PatchMethod = Helpers.GetMethod(typeof(Subverse), "Prefix");
        }

        if (Target != null && PatchMethod != null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Subversion", $"Hooking {Target.Name} with {PatchMethod.Name}");
            Manual.Patch(Target, PatchMethod,
                !MarseyConf.SubvertPreload ? HarmonyPatchType.Postfix : HarmonyPatchType.Prefix);

            return;
        }

        MarseyLogger.Log(MarseyLogger.LogType.ERRO, "Subverter failed load!");
    }

    private static void Postfix(object __instance)
    {
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Subversion", "Detour");
        MethodInfo? loadGameAssemblyMethod =
            AccessTools.Method(AccessTools.TypeByName("Robust.Shared.ContentPack.BaseModLoader"), "InitMod");

        if (loadGameAssemblyMethod == null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.FATL, "Subversion", "Failed to find InitMod method.");
            return;
        }

        foreach (string path in _subverters!)
        {
            Assembly subverterAssembly = Assembly.LoadFrom(path);
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Subversion", $"Sideloading {path}");
            AssemblyFieldHandler.InitLogger(subverterAssembly, subverterAssembly.FullName);

            loadGameAssemblyMethod.Invoke(__instance, new object[] { subverterAssembly });

            MethodInfo? entryMethod = CheckEntry(subverterAssembly);
            if (entryMethod != null)
            {
                Doorbreak.Enter(entryMethod);
            }

            Subverter.Hide(subverterAssembly);
        }
    }

    private static MethodInfo? CheckEntry(Assembly assembly)
    {
        MethodInfo? entryMethod = AssemblyFieldHandler.GetEntry(assembly);
        return entryMethod;
    }
}
