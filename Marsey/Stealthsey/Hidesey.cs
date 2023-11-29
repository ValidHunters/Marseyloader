using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using Marsey.Handbreak;
using Marsey.Subversion;

namespace Marsey.Stealthsey;

/// <summary>
/// Hides marseys from the game
/// </summary>
public static class Hidesey
{
    private static List<Assembly> _hideseys = new List<Assembly>();

    /// <summary>
    /// Hides 0Harmony from assembly list
    /// Finally, a patch loader that loads with a patch
    /// </summary>
    public static void Initialize()
    {
        Hide("0Harmony");
        AppDomain.CurrentDomain.GetAssemblies();
        // Is it really insane to patch system functions?
        MethodInfo? target = typeof(AppDomain)
            .GetMethod("GetAssemblies", BindingFlags.Public | BindingFlags.Instance);
        MethodInfo? postfix =
            typeof(HideseyPatches)
                .GetMethod("LieLoader", BindingFlags.Public | BindingFlags.Static);
        
        if (target == null || postfix == null) return;
        
        Manual.ManualPostfix(target, postfix);
        
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Hidesey started.");
    }
    
    /// <summary>
    /// Add assembly to _hideseys list
    /// </summary>
    /// <param name="marsey">string of assembly name</param>
    public static void Hide(string marsey)
    {
        Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly asm in asms)
        {
            if (asm.FullName != null && asm.FullName.Contains(marsey))
            {
                Hide(asm);
                return;
            }
        }
    }
    
    /// <summary>
    /// If we have the assembly object
    /// </summary>
    /// <param name="marsey">marsey assembly</param>
    public static void Hide(Assembly marsey)
    {
        _hideseys.Add(marsey);
    }
    
    public static Assembly[] LyingDomain(Assembly[] original)
    {
        return original.Where(assembly => !_hideseys.Contains(assembly)).ToArray();
    }
}
