using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using HarmonyLib;
using Marsey.Handbrake;

namespace Marsey.Stealthsey;

public enum HideLevel
{
    // Note that this only protects you from programmatic checks.

    /// <summary>
    /// Hidesey is disabled.
    /// </summary>
    /// <remarks>
    /// Servers with engine version 183.0.0 or above crash the client.
    /// </remarks>
    Disabled = 0,
    /// <summary>
    /// <para>Patcher is hidden from the game programmatically.</para>
    /// <para>Patches are not hidden for cases when an admin wants to know *what* patches are you running, rather than if you have any.</para>
    /// </summary>
    Duplicit = 1,
    /// <summary>
    /// Patcher and patches are hidden from the game programmatically
    /// </summary>
    Normal = 2,
    /// <summary>
    /// <para>Patcher and patches are hidden from the game programmatically</para>
    /// <para>Patcher does not log anything</para>
    /// <para>Separate logging is disabled</para>
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
    {                               // Five patches even
        MarseyVars.MarseyHide = GetHideseyLevel();
        
        if (MarseyVars.MarseyHide == HideLevel.Disabled) return;

        Disperse();
        
        Facade.Imposition("Marsey");

        Perjurize(); // Patch detection methods

        MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"Hidesey started. Running {MarseyVars.MarseyHide.ToString()} configuration.");
    }

    /// <summary>
    /// General function to hide assemblies matching name from detection methods
    /// </summary>
    /// <remarks>Because certain assemblies are loaded later this is called twice</remarks>
    public static void Disperse()
    {
        if (MarseyVars.MarseyHide == HideLevel.Disabled) return;
        
        Hide("0Harmony"); // https://github.com/space-wizards/RobustToolbox/blob/962f5dc650297b883e8842aea8b41393d4808ac9/Robust.Client/GameController/GameController.Standalone.cs#L77
        Hide("Mono.Cecil");
        Hide("MonoMod", true);
        Hide("MonoMod.Iced");
        Hide("System.Reflection.Emit,");
    }

    /// <summary>
    /// Add assembly to _hideseys list
    /// </summary>
    /// <param name="marsey">string of assembly name</param>
    private static void Hide(string marsey, bool recursive = false)
    {
        Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly asm in asms)
        {
            if (asm.FullName == null || !asm.FullName.Contains(marsey)) continue;
            Hide(asm);
            if (!recursive) return;
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

        target = typeof(AssemblyLoadContext).GetProperty("Assemblies")!.GetGetMethod();
        postfix = typeof(HideseyPatches).GetMethod("LieContext", BindingFlags.Public | BindingFlags.Static)!;
        if (target != null) Manual.Postfix(target, postfix);
        
        target = typeof(AssemblyLoadContext).GetProperty("All")!.GetGetMethod();
        postfix = typeof(HideseyPatches).GetMethod("LieManifest", BindingFlags.Public | BindingFlags.Static)!;
        if (target != null) Manual.Postfix(target, postfix);
    }

    /// <summary>
    /// Checks HideLevel env variable, defaults to Normal
    /// </summary>
    private static HideLevel GetHideseyLevel()
    {
        string envVar = Environment.GetEnvironmentVariable("MARSEY_HIDE_LEVEL")!;
        
        if (int.TryParse(envVar, out int hideLevelValue) && Enum.IsDefined(typeof(HideLevel), hideLevelValue)) 
            return (HideLevel)hideLevelValue;
        
        return HideLevel.Normal;
    }
    
    /// <summary>
    /// Returns a list of only assemblies that are not hidden from a given list
    /// </summary>
    public static Assembly[] LyingDomain(Assembly[] original)
    {
        return original.Where(assembly => !_hideseys.Contains(assembly)).ToArray();
    }

    public static IEnumerable<Assembly> LyingContext(IEnumerable<Assembly> original)
    {
        return original.Where(assembly => !_hideseys.Contains(assembly));
    }

    public static IEnumerable<AssemblyLoadContext> LyingManifest(IEnumerable<AssemblyLoadContext> original)
    {
        return original.Where(context => context.Name != "Assembly.Load(byte[], ...)");
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