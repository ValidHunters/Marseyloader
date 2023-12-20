using System.Reflection;
using HarmonyLib;
using Marsey.GameAssembly;
using Marsey.Handbrake;
using Marsey.Misc;
using static System.Boolean;

namespace Marsey.Stealthsey.Game;

public static class DiscordRPC
{
    private static string EnvName = "MARSEY_DISABLE_PRESENCE";
    
    public static void Disable()
    {
        if (!CheckEnv()) return;
        
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "DiscordRPC", "Disabling.");
        
        Assembly RC = GameAssemblies.RobustClient!;
        Type? DRPC = RC.GetType("Robust.Client.Utility.DiscordRichPresence");
        if (DRPC == null) return;
        MethodInfo? DRPCinit = DRPC.GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public);
        if (DRPCinit == null) return;

        MethodInfo PrefSkip = typeof(HideseyPatches).GetMethod("Skip", BindingFlags.Public | BindingFlags.Static)!;
        
        Manual.Patch(DRPCinit, PrefSkip, HarmonyPatchType.Prefix);
    }

    private static bool CheckEnv()
    {
        bool toggle;
        string envVal = Environment.GetEnvironmentVariable(EnvName);
        Envsey.CleanFlag(EnvName);
        TryParse(envVal, out toggle);
        return toggle;
    }
}