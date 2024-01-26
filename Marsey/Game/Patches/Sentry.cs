using System.Reflection;
using HarmonyLib;
using Marsey.Game.Misc;
using Marsey.Handbreak;
using Marsey.Misc;

namespace Marsey.Game.Patches;

/// <summary>
/// Hooks to EntryPoint
/// Signals to Marsey when the game proper is going to start
/// </summary>
public static class Sentry
{
    private static bool _starting;
    public static bool State => _starting;
    
    public static void Patch()
    {
        MethodInfo? PrefEP = typeof(Sentry).GetMethod("PrefBLR", BindingFlags.Static | BindingFlags.NonPublic);
        
        Type EP = AccessTools.TypeByName("Robust.Shared.ContentPack.BaseModLoader");
        MethodInfo? InitMi = AccessTools.Method(EP, "BroadcastRunLevel");

        Manual.Patch(InitMi, PrefEP, HarmonyPatchType.Prefix);
    }

    private static void PrefBLR(ref object level)
    {
        if (level is not Enum || Convert.ToInt32(level) != 1) return; // ModRunLevel.Init
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Sentry set");
        _starting = true;
    }
}