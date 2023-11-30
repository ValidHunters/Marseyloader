using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;
using Marsey.Handbrake;
using Marsey.Subversion;

namespace Marsey.Stealthsey;

/// <summary>
/// Hides marseys from the game
/// </summary>
public static class Hidesey
{
    private static List<Assembly> _hideseys = new List<Assembly>();

    /// <summary>
    /// Starts Hidesey. Patches GetAssemblies, GetReferencedAssemblies and hides Harmony from assembly list.
    /// </summary>
    public static void Initialize() // Finally, a patch loader that loads with a patch
    {                               // Two patches even
        Hide("0Harmony");    // https://github.com/space-wizards/RobustToolbox/blob/962f5dc650297b883e8842aea8b41393d4808ac9/Robust.Client/GameController/GameController.Standalone.cs#L77
        // Is it really insane to patch system functions?
        MethodInfo? target = typeof(AppDomain)
            .GetMethod("GetAssemblies", BindingFlags.Public | BindingFlags.Instance);
        
        MethodInfo? postfix =
            typeof(HideseyPatches)
                .GetMethod("LieLoader", BindingFlags.Public | BindingFlags.Static)!;
        
        if (target == null) return;
        Manual.Postfix(target, postfix);
        
        target = 
            Assembly.GetExecutingAssembly().GetType()
                .GetMethod("GetReferencedAssemblies");
         
        postfix =
            typeof(HideseyPatches)
                .GetMethod("LieReference", BindingFlags.Public | BindingFlags.Static)!;
        
         if (target == null) return;
         Manual.Postfix(target, postfix);

         MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Hidesey started.");
    }
    
    /// <summary>
    /// Add assembly to _hideseys list
    /// </summary>
    /// <param name="marsey">string of assembly name</param>
    private static void Hide(string marsey)
    {
        Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly asm in asms)
        {
            if (asm.FullName == null || !asm.FullName.Contains(marsey)) continue;
            
            Hide(asm);
            return;
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
    
    /// <summary>
    /// Returns a list of only assemblies that are not hidden from a given list
    /// </summary>
    public static Assembly[] LyingDomain(Assembly[] original)
    {
        return original.Where(assembly => !_hideseys.Contains(assembly)).ToArray();
    }
    
    /// <summary>
    /// Returns a list of only assemblynames that are not hidden from a given list
    /// </summary>
    public static AssemblyName[] LyingReference(AssemblyName[] original)
    {
        List<string?> hideseysNames = _hideseys.Select(a => a.GetName().Name).ToList();
        AssemblyName[] result = original.Where(assembly => !hideseysNames.Contains(assembly.Name)).ToArray();
        return result;
    }
}
