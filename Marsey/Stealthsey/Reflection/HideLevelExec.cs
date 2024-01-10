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
public static class HideLevelExec
{
    /// <summary>
    /// Initializes the HideLevelExec by finding and patching methods with HideLevelRequirement attributes.
    /// </summary>
    public static void Initialize()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        IEnumerable<Type> types = assembly.GetTypes();
        
        IEnumerable<Type> marseyTypes = types.Where(t => t.Namespace != null && t.Namespace.StartsWith("Marsey"));
        
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
        IEnumerable<MethodInfo> methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance);

        foreach (MethodInfo method in methods)
        {
            ExecIfLevelMet(method);
        }
    }
    
    /// <summary>
    /// Executes the method if the current hide level meets or exceeds the required level specified by the HideLevelRequirement attribute.
    /// </summary>
    private static void ExecIfLevelMet(MethodInfo method)
    {
        HideLevelRequirement? hideLevelRequirement = method.GetCustomAttribute<HideLevelRequirement>();
        HideLevelRestriction? hideLevelRestriction = method.GetCustomAttribute<HideLevelRestriction>();

        if (hideLevelRequirement == null && hideLevelRestriction == null) return;
        
        MethodInfo? prefix = typeof(HideseyPatches).GetMethod("LevelCheck", BindingFlags.Public | BindingFlags.Static);
        Manual.Patch(method, prefix, HarmonyPatchType.Prefix);
    }
}