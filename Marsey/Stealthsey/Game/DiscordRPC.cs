using System.Reflection;
using HarmonyLib;
using Marsey.Config;
using Marsey.GameAssembly;
using Marsey.Handbrake;
using Marsey.Misc;

namespace Marsey.Stealthsey.Game;

public static class DiscordRPC
{
    public static void Disable()
    {
        if (!MarseyConf.KillRPC) return;
        
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "DiscordRPC", "Disabling.");
        
        Assembly RC = GameAssemblies.RobustClient!;
        Type? DRPC = RC.GetType("Robust.Client.Utility.DiscordRichPresence");
        if (DRPC == null) return;
        MethodInfo? DRPCinit = DRPC.GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public);
        if (DRPCinit == null) return;

        MethodInfo PrefSkip = typeof(HideseyPatches).GetMethod("Skip", BindingFlags.Public | BindingFlags.Static)!;
        
        Manual.Patch(DRPCinit, PrefSkip, HarmonyPatchType.Prefix);
    }
}