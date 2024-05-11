using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
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
/// Manages the sideloader (Subversion)
/// </summary>
public static class Subverse
{
    private static List<string>? _subversionPaths = null;
    private static SubversionQueue _subversions = new SubversionQueue();

    /// <summary>
    /// Check if we have any subversions enabled
    /// </summary>
    public static bool CheckSubversions()
    {
        _subversionPaths = Marserializer.Deserialize(new string[]{MarseyVars.MarseyPatchFolder}, Subverter.MarserializerFile);

        return _subversionPaths != null && _subversionPaths.Count != 0;
    }

    /// <summary>
    /// Patches subverter ahead of everything else
    /// This is done as we attach to the assembly loading function
    /// </summary>
    public static void PatchSubverter()
    {
        InitLists();

        MethodInfo? Target = Helpers.GetMethod("Robust.Shared.ContentPack.ModLoader", "TryLoadModules");
        MethodInfo? UniFix = Helpers.GetMethod(typeof(Subverse), "UniHook");

        if (Target != null && UniFix != null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Subversion", $"Hooking {Target.Name} with {UniFix.Name}");
            Manual.Patch(Target, UniFix, HarmonyPatchType.Prefix);
            Manual.Patch(Target, UniFix, HarmonyPatchType.Postfix);
            return;
        }

        MarseyLogger.Log(MarseyLogger.LogType.ERRO, "Subverter failed load!");
    }

    private static void InitLists()
    {
        foreach (string path in _subversionPaths!)
        {
            Assembly subverterAssembly = Assembly.LoadFrom(path);
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Subversion", $"Loading {path}");

            // The only way this assert would not be true is if you manually put the path to a dll in the json list
            Type patch = AssemblyInitializer.GetDataType(subverterAssembly)!;
            //                                   Should be preloading by default
            if (AssemblyFieldHandler.DeterminePreload(patch, missing: true)) _subversions.Preload.Add(subverterAssembly);
            else _subversions.Postload.Add(subverterAssembly);
        }
    }


    private static bool _firstPassed = false;
    [UsedImplicitly]
    private static void UniHook(object __instance)
    {
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Subversion", "Detour");
        MethodInfo? loadGameAssemblyMethod = AccessTools.Method(AccessTools.TypeByName("Robust.Shared.ContentPack.BaseModLoader"), "InitMod");

        if (loadGameAssemblyMethod == null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.FATL, "Subversion", "Failed to find InitMod method.");
            return;
        }


        List<Assembly> queue;
        queue = _firstPassed ? _subversions.Preload : _subversions.Postload;

        foreach (Assembly sideload in queue)
        {
            AssemblyFieldHandler.InitLogger(sideload, sideload.FullName);

            loadGameAssemblyMethod.Invoke(__instance, new object[] { sideload });

            MethodInfo? entryMethod = CheckEntry(sideload);
            if (entryMethod != null)
            {
                Doorbreak.Enter(entryMethod);
            }

            Sedition.Queue(sideload);
        }

        _firstPassed = true;
    }

    private static MethodInfo? CheckEntry(Assembly assembly)
    {
        MethodInfo? entryMethod = AssemblyFieldHandler.GetEntry(assembly);
        return entryMethod;
    }
}

internal class SubversionQueue
{
    public List<Assembly> Preload { get; set; } = new List<Assembly>();
    public List<Assembly> Postload { get; set; } = new List<Assembly>();
}
