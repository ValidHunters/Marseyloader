using System.Reflection;
using HarmonyLib;
using Marsey.Config;
using Marsey.Handbrake;
using Marsey.Misc;

namespace Marsey.GameAssembly.Patches;

/// <summary>
/// Whitelists command execution that is dependent on server
/// </summary>
/// <remarks>This breaks features that rely on this, for example White Dream's "notice" command. Mileage may vary.</remarks>
public static class Blackhole
{
    private static List<string> AllowedCommands = ["observe", "joingame", "ghostroles", "openahelp", "deadmin", "readmin"];

    public static void Patch()
    {
        if (!MarseyConf.DisableREC) return;

        Type? CCH = AccessTools.TypeByName("Robust.Client.Console.ClientConsoleHost");
        MethodInfo? REC = AccessTools.Method(CCH, "RemoteExecuteCommand");
        MethodInfo RECpf = AccessTools.Method(typeof(Blackhole), "RECPref");

        if (REC == null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.WARN, "Blackhole", "RemoteExecuteCommand not found, not patching.");
            return;
        }
        
        Manual.Patch(REC, RECpf, HarmonyPatchType.Prefix);
    }

    private static bool RECPref(ref dynamic? session, ref string command)
    {
        if (session is null) return true;
        
        foreach (string comm in AllowedCommands)
        {
            if (command.StartsWith(comm + " "))
                return true;
        }
        
        MarseyLogger.Log(MarseyLogger.LogType.INFO, "Blackhole", $"Blocked \"{command}\" from executing.");
        return false;
    }
}