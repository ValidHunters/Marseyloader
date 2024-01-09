using System.Reflection;
using HarmonyLib;
using Marsey.Config;
using Marsey.Handbrake;
using Marsey.Misc;
using Marsey.Stealthsey;

namespace Marsey.GameAssembly.Patches;

public static class Dumper
{
    private static string path = "";
    private static short counter = 0;
    
    public static void Patch()
    {
        if (!MarseyConf.DumpAssemblies) return;
        
        GetExactPath();
        Type? ModLoader = AccessTools.TypeByName("Robust.Shared.ContentPack.ModLoader");
        
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Dumper", "Patching TLM");
        MethodInfo? TryLoadModules = AccessTools.Method(ModLoader, "TryLoadModules");
        MethodInfo? TLMPmi = typeof(Dumper).GetMethod("TLMPostfix", BindingFlags.NonPublic | BindingFlags.Static);
        Manual.Patch(TryLoadModules, TLMPmi, HarmonyPatchType.Postfix);
        
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Dumper", "Patching LGA");
        MethodInfo? LoadGameAssembly = AccessTools.Method(ModLoader, "LoadGameAssembly", new[] { typeof(Stream), typeof(Stream), typeof(bool) });
        MethodInfo? LGAPmi = typeof(Dumper).GetMethod("LGAPrefix", BindingFlags.NonPublic | BindingFlags.Static);
        Manual.Patch(LoadGameAssembly, LGAPmi, HarmonyPatchType.Prefix);
    }
    
    /// <summary>
    /// Intercept assemblies, save to disk
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private static bool LGAPrefix(ref Stream assembly)
    {
        FileHandler.SaveAssembly(path, $"{counter}.dll", assembly);
        counter++;

        return true;
    }
    
    /// <summary>
    /// Assemblies finished loading, close the game
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private static void TLMPostfix()
    {
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Dumper", $"Dumps saved to {path}");
        MarseyLogger.Log(MarseyLogger.LogType.INFO, "Dumper", $"Exited with {counter} assemblies.");
        Environment.Exit(0);
    }

    private static void GetExactPath()
    {
        string fork = Environment.GetEnvironmentVariable("MARSEY_DUMP_FORKID") ?? "custom";
        Envsey.CleanFlag(fork);

        string loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        path = Path.Combine(loc, "Dumper/", $"{fork}/");
    }
}