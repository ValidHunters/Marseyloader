using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using HarmonyLib;
using Marsey.Config;
using Marsey.Game.Patches;
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
    // But so far nobody was told to hop on vc to stream their game

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
    private static bool _initialized;
    private static bool _caching;

    /// <summary>
    /// Starts Hidesey. Patches GetAssemblies, GetReferencedAssemblies and hides Harmony from assembly list.
    /// Requires MarseyHide to not be Disabled.
    /// </summary>
    public static void Initialize() // Finally, a patch loader that loads with a patch
    {                                                           // Five patches even
        if (_initialized)
            return;

        _initialized = true;

        HideseyAttributeManager.Initialize();

        Load();
    }

    [HideLevelRequirement(HideLevel.Duplicit)]
    private static void Load()
    {
        Disperse();

        Facade.Imposition("Marsey");

        Perjurize(); // Patch detection methods

        MarseyLogger.Log(MarseyLogger.LogType.INFO, $"Hidesey started. Running {MarseyConf.MarseyHide.ToString()} configuration.");
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
        DiscordRPC.Patch();

        // Cleanup
        Disperse();
    }

    /// <summary>
    /// This gets executed after MarseyPatcher finished its job.
    /// </summary>
    public static void Cleanup()
    {
        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            ToggleCaching();
        });
    }

    private static void ToggleCaching()
    {
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"Caching is set to {!_caching}");
        _caching = !_caching;
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
        MethodInfo? Lie = Helpers.GetMethod(typeof(HideseyPatches), "Lie");

        (MethodInfo?, Type)[] patches =
        [
            (typeof(AppDomain).GetMethod(nameof(AppDomain.GetAssemblies)), typeof(Assembly[])),
            (Assembly.GetExecutingAssembly().GetType().GetMethod(nameof(Assembly.GetReferencedAssemblies)), typeof(AssemblyName[])),
            (typeof(Assembly).GetMethod(nameof(Assembly.GetTypes)), typeof(Type[])),
            (typeof(AssemblyLoadContext).GetProperty("Assemblies")?.GetGetMethod(), typeof(IEnumerable<Assembly>)),
            (typeof(AssemblyLoadContext).GetProperty("All")?.GetGetMethod(), typeof(IEnumerable<AssemblyLoadContext>))
        ];

        foreach ((MethodInfo? targetMethod, Type returnType) in patches)
        {
            Helpers.PatchGenericMethod(
                target: targetMethod,
                patch: Lie,
                patchReturnType: returnType,
                patchType: HarmonyPatchType.Postfix
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
        if (!_caching)
            return original.Except(hiddentypes).ToArray();

        Type[] cached = Facade.Cached;

        if (cached != Array.Empty<Type>()) return cached;

        cached = original.Except(hiddentypes).ToArray();
        Facade.Cache(cached);

        return cached;
    }

    #endregion

    /// <summary>
    /// Checks if the call was made by or concerns the content pack
    /// </summary>
    internal static bool FromContent()
    {
        StackTrace stackTrace = new();
        //MarseyLogger.Log(MarseyLogger.LogType.TRCE, "Veil", $"Stacktrace check called, given {stackTrace.GetFrames().Length} frames.");

        foreach (StackFrame frame in stackTrace.GetFrames())
        {
            MethodBase? method = frame.GetMethod();
            if (method == null || method.DeclaringType == null) continue;
            string? namespaceName = method.DeclaringType.Namespace;
            if (!string.IsNullOrEmpty(namespaceName) && namespaceName.StartsWith("Content."))
            {
                //MarseyLogger.Log(MarseyLogger.LogType.INFO, "Veil", "Hidden types from a contentpack check!");
                return true;
            }
        }

        return false;
    }

}
