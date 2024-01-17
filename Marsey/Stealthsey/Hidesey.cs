using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using HarmonyLib;
using Marsey.Config;
using Marsey.GameAssembly.Patches;
using Marsey.Handbreak;
using Marsey.Misc;
using Marsey.Stealthsey.Reflection;

namespace Marsey.Stealthsey;

/// <summary>
/// Level of concealment from the game
/// </summary>
public enum HideLevel
{
    // Note that this only protects you from programmatic checks.

    /// <summary>
    /// Hidesey is disabled. No measures are taken to hide the patcher or patches.
    /// </summary>
    /// <remarks>
    /// Clients on engine version 183.0.0 or above will experience crashes.
    /// </remarks>
    Disabled = 0,
    /// <summary>
    /// Patcher is hidden from the game.
    /// </summary>
    /// <remarks>
    /// Patches remain visible to allow administrators to inspect which patches are being used.
    /// This is the "friend server" option.
    /// </remarks>
    Duplicit = 1,
    /// <summary>
    /// Patcher and patches are hidden.
    /// </summary>
    /// <remarks>
    /// This is the default option.
    /// </remarks>
    Normal = 2,
    /// <summary>
    /// Patcher and patches are hidden.
    /// Separate patch logging is disabled.
    /// </summary>
    Explicit = 3,
    /// <summary>
    /// Patcher, patches are hidden.
    /// Separate patch logging is disabled.
    /// Subversion patches are hidden and cannot be reflected by the game.
    /// </summary>
    Unconditional = 4
}

/// <summary>
/// Hides patches from the game
/// </summary>
public static class Hidesey
{
    private static List<Assembly> _hideseys = new List<Assembly>();
    private static bool _initialized = false;

    /// <summary>
    /// Starts Hidesey. Patches GetAssemblies, GetReferencedAssemblies and hides Harmony from assembly list.
    /// Requires MarseyHide to not be Disabled.
    /// </summary>
    public static void Initialize() // Finally, a patch loader that loads with a patch
    {                                                           // Five patches even
        if (_initialized)
            return;

        _initialized = true;
        
        MarseyConf.MarseyHide = GetHideseyLevel();
        HideLevelExec.Initialize();
        
        Load();
    }

    [HideLevelRequirement(HideLevel.Duplicit)]
    private static void Load()
    {
        Disperse();
        
        Facade.Imposition("Marsey");

        Perjurize(); // Patch detection methods
        
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"Hidesey started. Running {MarseyConf.MarseyHide.ToString()} configuration.");
    }

    /// <summary>
    /// General function to hide assemblies matching name from detection methods
    /// Requires MarseyHide to not be Disabled.
    /// </summary>
    /// <remarks>Because certain assemblies are loaded later this is called twice</remarks>
    [HideLevelRequirement(HideLevel.Duplicit)]
    public static void Disperse()
    {
        (string, bool)[] assembliesToHide = new (string, bool)[]
        {
            ("0Harmony", false),
            ("Mono.Cecil", false),
            ("MonoMod", true),
            ("MonoMod.Iced", false),
            ("System.Reflection.Emit,", false),
            ("Marsey", false),
            ("Harmony", true)
        };

        foreach ((string assembly, bool recursive) in assembliesToHide)
        {
            Hide(assembly, recursive);
        }
    }

    /// <summary>
    /// This gets executed after game assemblies have been loaded into the appdomain.
    /// </summary>
    public static void PostLoad()
    {
        HWID.Force();
        DiscordRPC.Disable();
        
        // Cleanup
        Disperse();
    }

    /// <summary>
    /// This gets executed after MarseyPatcher finished its job.
    /// </summary>
    public static void Cleanup()
    {
        // Include methods here if needed
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
    private static void Hide(Assembly marsey)
    {
        Facade.Cloak(marsey);
        _hideseys.Add(marsey);
    }

    /// <summary>
    /// Entrypoint for patch assemblies
    /// Requires HideLevel to be Normal or above
    /// </summary>
    /// <param name="marsey">Assembly from a patch</param>
    [HideLevelRequirement(HideLevel.Normal)]
    public static void HidePatch(Assembly marsey)
    {
        Hide(marsey);
    }

    /// <summary>
    /// Undermines system functions, hides what doesnt belong from view
    /// </summary>
    /// <exception cref="HideseyException">Thrown if ThrowOnFail is true and any of the patches fails to apply</exception>
    private static void Perjurize()
    {
        (Type, string, Type)[] patches =
        {
            (typeof(AppDomain), nameof(AppDomain.GetAssemblies), typeof(Assembly[])),
            (Assembly.GetExecutingAssembly().GetType(), nameof(Assembly.GetReferencedAssemblies), typeof(AssemblyName[])),
            (typeof(Assembly), nameof(Assembly.GetTypes), typeof(Type[])),
            (typeof(AssemblyLoadContext), "Assemblies", typeof(IEnumerable<Assembly>)),
            (typeof(AssemblyLoadContext), "All", typeof(IEnumerable<AssemblyLoadContext>))
        };

        foreach ((Type? targetType, string methodName, Type returnType) in patches)
        {
            Helpers.PatchGenericMethod(
                targetType: targetType, 
                targetMethodName: methodName, 
                patchType: typeof(HideseyPatches),
                patchMethodName: "Lie", 
                returnType: returnType, 
                patchingType: HarmonyPatchType.Postfix
                );
        }
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

    #region LyingPatches

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
        IEnumerable<Type> hiddentypes = Facade.GetTypes();
        return original.Except(hiddentypes).ToArray();
    }

    #endregion

}