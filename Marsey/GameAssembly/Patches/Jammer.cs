using System.Reflection;
using HarmonyLib;
using Marsey.Config;
using Marsey.Handbrake;
using Marsey.Misc;
using Marsey.Stealthsey;

namespace Marsey.GameAssembly.Patches;

/// <summary>
/// Disable game's redialing feature, making sure you wont get sent to vore station if admins don't like you.
/// </summary>
/// <remarks>While redialing "for the funny" is not allowed by wizard's den you are not able to prove anything because its not even logged in a verifiable manner.</remarks>
/// <remarks><para>Not to be confused with Stealthsey.Redial.</para></remarks>
public static class Jammer
{
    public static void Patch()
    {
        if (!MarseyConf.JamDials) return;
        
        MarseyLogger.Log(MarseyLogger.LogType.INFO, "Jammer", "Disabling redialer");
        
        Type? GameCont = AccessTools.TypeByName("Robust.Client.GameController");
        MethodInfo? Redial = AccessTools.Method(GameCont, "Redial");
        MethodInfo? RedialSkip = typeof(Jammer).GetMethod("Disable", BindingFlags.NonPublic | BindingFlags.Static);

        if (Redial == null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.WARN, "Jammer", "Couldn't get Redial method handle. Not patching.");
            return;
        }
        
        Manual.Patch(Redial, RedialSkip, HarmonyPatchType.Prefix);
    }

    private static bool Disable()
    {
        MarseyLogger.Log(MarseyLogger.LogType.INFO, "Jammer", "Blocked a redial!");
        return false;
    }
}