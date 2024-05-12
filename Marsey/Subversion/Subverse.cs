using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Marsey.Config;
using Marsey.Game.Managers;
using Marsey.Game.Misc;
using Marsey.Handbreak;
using Marsey.Misc;
using Marsey.PatchAssembly;
using Marsey.Serializer;

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

        //MethodInfo? Target = Helpers.GetMethod(, "TryLoadModules");
        MethodInfo? Target = Helpers.GetMethod("Robust.Shared.ContentPack.ModLoader", "TryLoadModules");
        MethodInfo? Prefix = Helpers.GetMethod(typeof(Subverse), "Prefix");
        MethodInfo? Postfix = Helpers.GetMethod(typeof(Subverse), "Postfix");


        if (Target != null && Prefix != null && Postfix != null)
        {
            // We are required to patch both prefix and postfix at once
            // TODO: rewrite Handbreak.Manual for that
            Harmony e = HarmonyManager.GetHarmony();
            e.Patch(Target, prefix: Prefix, postfix: Postfix);
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
            if (AssemblyFieldHandler.DeterminePreload(patch, missing: true))
                _subversions.Preload.Add(subverterAssembly);
            else
                _subversions.Postload.Add(subverterAssembly);
        }
    }

    private static MethodInfo? _lGAMi = null;
    private static object? _instance = null;
    [UsedImplicitly]
    private static void Prefix(ref object __instance, out bool __state)
    {
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Subversion", "Detour");
        _lGAMi = AccessTools.Method(AccessTools.TypeByName("Robust.Shared.ContentPack.BaseModLoader"), "InitMod");
        _instance = __instance;

        if (_lGAMi == null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.FATL, "Subversion", "Failed to find InitMod method.");
            __state = false;
            return;
        }

        __state = true;
        Sideload(_subversions.Preload);
    }

    [UsedImplicitly]
    private static void Postfix(bool __state)
    {
        if (!__state) return;

        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(0.2));
            if (GameAssemblies.ClientInitialized())
                Sideload(_subversions.Postload);
        });
    }

    private static void Sideload(List<Assembly> assemblies)
    {
        foreach (Assembly sideload in assemblies)
        {
            AssemblyFieldHandler.InitLogger(sideload, sideload.FullName);

            _lGAMi!.Invoke(_instance, new object[] { sideload });

            MethodInfo? entryMethod = CheckEntry(sideload);
            if (entryMethod != null)
            {
                Doorbreak.Enter(entryMethod);
            }

            Sedition.Queue(sideload);
        }
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
