using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using HarmonyLib;
using Marsey.Config;
using Marsey.Handbrake;
using Marsey.Misc;
using Marsey.Stealthsey.Game;
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
    /// Subversion is disabled.
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
        
        MarseyVars.MarseyHide = GetHideseyLevel();
        HideLevelExec.Initialize();
        
        Load();
    }

    [HideLevelRequirement(HideLevel.Duplicit)]
    private static void Load()
    {
        Disperse();
        
        Facade.Imposition("Marsey");

        Perjurize(); // Patch detection methods
        
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"Hidesey started. Running {MarseyVars.MarseyHide.ToString()} configuration.");
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Retrying.");
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
        (MethodInfo?, MethodInfo?)[] postfixPatches = new (MethodInfo?, MethodInfo?)[]
        {
            (
                typeof(AppDomain).GetMethod("GetAssemblies", BindingFlags.Public | BindingFlags.Instance), 
                typeof(HideseyPatches).GetMethod("LieLoader", BindingFlags.Public | BindingFlags.Static)
            ),
            (
                Assembly.GetExecutingAssembly().GetType().GetMethod("GetReferencedAssemblies"), 
                typeof(HideseyPatches).GetMethod("LieReference", BindingFlags.Public | BindingFlags.Static)
            ),
            (
                typeof(Assembly).GetMethod("GetTypes"), 
                typeof(HideseyPatches).GetMethod("LieTyper", BindingFlags.Public | BindingFlags.Static)
            ),
            (
                typeof(AssemblyLoadContext).GetProperty("Assemblies")!.GetGetMethod(), 
                typeof(HideseyPatches).GetMethod("LieContext", BindingFlags.Public | BindingFlags.Static)
            ),
            (
                typeof(AssemblyLoadContext).GetProperty("All")!.GetGetMethod(), 
                typeof(HideseyPatches).GetMethod("LieManifest", BindingFlags.Public | BindingFlags.Static)
            )
        };

        foreach ((MethodInfo? original, MethodInfo? patch) in postfixPatches)
        {
            if (original != null && patch != null)
            {
                Manual.Patch(original, patch, HarmonyPatchType.Postfix);
            }
            else
            {
                string message = $"Failed to patch {original?.Name} using {patch?.Name}";

                // Close client if any of the hidesey patches fail
                if (MarseyVars.ThrowOnFail)
                    throw new HideseyException(message);
            
                MarseyLogger.Log(MarseyLogger.LogType.FATL, message);
            }
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