using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Marsey.Config;
using Marsey.Handbreak;
using Marsey.Misc;

namespace Marsey.Stealthsey.Reflection;

/// <summary>
/// Manages methods with the HideLevelRequirement attribute, patching them with a prefix.
/// </summary>
public static class HideseyAttributeManager
{
    /// <summary>
    /// Initializes the HideLevelExec by finding and patching methods with HideLevelRequirement attributes.
    /// </summary>
    public static void Initialize()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        IEnumerable<Type> types = assembly.GetTypes();

        IEnumerable<Type> marseyTypes = Assembly.GetExecutingAssembly().ExportedTypes;

        foreach (Type type in marseyTypes)
        {
            CheckAndExecute(type);
        }
    }

    /// <summary>
    /// Checks each type for methods with HideLevelRequirement attributes and executes them if the hide level is met.
    /// </summary>
    private static void CheckAndExecute(Type type)
    {
        // Get all methods from the given type
        IEnumerable<MethodInfo> methods = AccessTools.GetDeclaredMethods(type);

        foreach (MethodInfo method in methods)
        {
            SetExecLevels(method);
            SetPatchless(method);
        }
    }

    /// <summary>
    /// Executes the method if the current hide level meets or exceeds the required level specified by the HideLevelRequirement attribute.
    /// </summary>
    private static void SetExecLevels(MethodInfo method)
    {
        HideLevelRequirement? hideLevelRequirement = method.GetCustomAttribute<HideLevelRequirement>();
        HideLevelRestriction? hideLevelRestriction = method.GetCustomAttribute<HideLevelRestriction>();

        if (hideLevelRequirement == null && hideLevelRestriction == null) return;

        MethodInfo? prefix = typeof(HideseyPatches).GetMethod("LevelCheck", BindingFlags.Public | BindingFlags.Static);
        Manual.Patch(method, prefix, HarmonyPatchType.Prefix);
    }

    private static void SetPatchless(MethodInfo method)
    {
        if (method.GetCustomAttribute<Patching>() is null) return;
        if (method.IsGenericMethod) throw new InvalidOperationException("Patching attribute not allowed on generic methods.");

        MethodInfo? prefix = AccessTools.Method(typeof(HideseyPatches), nameof(HideseyPatches.SkipPatchless));

        Manual.Patch(method, prefix, HarmonyPatchType.Prefix);
    }
}
