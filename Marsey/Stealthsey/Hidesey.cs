using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Marsey.Handbrake;

namespace Marsey.Stealthsey;

public enum HideLevel
{
    // Note that this only protects you from programmatic checks.

    /// <summary>
    /// Hidesey is disabled.
    /// </summary>
    Disabled = 0,
    /// <summary>
    /// Patcher is hidden from the game programmatically
    /// </summary>
    /// <remarks>This is required for playing the game past engine version 183.0.0,
    /// As 0Harmony is detected by the game at runtime</remarks>
    Duplicit = 1,
    /// <summary>
    /// Patcher and patches are hidden from the game programmatically
    /// </summary>
    Normal = 2,
    /// <summary>
    /// <para>Patcher and patches are hidden from the game programmatically</para>
    /// <para>Patcher does not log anything</para>
    /// </summary>
    Explicit = 3,
    /// <summary>
    /// <para>Patcher and patches are hidden from the game programmatically</para>
    /// <para>Patcher does not log anything</para>
    /// <para>Preloads and subversions are disabled</para>
    /// </summary>
    Unconditional = 4
}

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
        string envVar = Environment.GetEnvironmentVariable("MARSEY_HIDE_LEVEL")!;
        if (int.TryParse(envVar, out int hideLevelValue) && Enum.IsDefined(typeof(HideLevel), hideLevelValue))
        {
            MarseyVars.MarseyHide = (HideLevel)hideLevelValue;
        }
        
        if (MarseyVars.MarseyHide == HideLevel.Disabled) return;
        
        Hide("0Harmony"); // https://github.com/space-wizards/RobustToolbox/blob/962f5dc650297b883e8842aea8b41393d4808ac9/Robust.Client/GameController/GameController.Standalone.cs#L77
        
        Facade.Imposition("Marsey");
        
        Perjurize(); // Patch detection methods

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
        Facade.Cloak(marsey);
        _hideseys.Add(marsey);
    }

    private static void Perjurize()
    {
        MethodInfo? target, postfix;

        target = typeof(AppDomain).GetMethod("GetAssemblies", BindingFlags.Public | BindingFlags.Instance);
        postfix = typeof(HideseyPatches).GetMethod("LieLoader", BindingFlags.Public | BindingFlags.Static)!;
        if (target != null) Manual.Postfix(target, postfix);

        target = Assembly.GetExecutingAssembly().GetType().GetMethod("GetReferencedAssemblies");
        postfix = typeof(HideseyPatches).GetMethod("LieReference", BindingFlags.Public | BindingFlags.Static)!;
        if (target != null) Manual.Postfix(target, postfix);

        target = typeof(Assembly).GetMethod("GetTypes");
        postfix = typeof(HideseyPatches).GetMethod("LieTyper", BindingFlags.Public | BindingFlags.Static)!;
        if (target != null) Manual.Postfix(target, postfix);
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

    /// <summary>
    /// Hides anything within
    /// </summary>
    public static Type[] LyingTyper(Type[] original)
    {
        Type[] hiddentypes = Facade.GetTypes();
        return original.Except(hiddentypes).ToArray();
    }

}