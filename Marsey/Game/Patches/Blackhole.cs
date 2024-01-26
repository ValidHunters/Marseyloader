using HarmonyLib;
using Marsey.Config;
using Marsey.Handbreak;
using Marsey.Misc;

namespace Marsey.Game.Patches;

/// <summary>
/// Whitelists command execution that is dependent on server
/// </summary>
/// <remarks>Breaks features that rely on this, for example White Dream's "notice" command. Mileage may vary.</remarks>
public static class Blackhole
{
    private static List<string> AllowedCommands = ["observe", "joingame", "ghostroles", "openahelp", "deadmin", "readmin", "say", "whisper"];

    public static void Patch()
    {
        if (!MarseyConf.DisableREC) return;

        MarseyLogger.Log(MarseyLogger.LogType.WARN, "Blackhole", "Blackholing RemoteExecutingCommand! This may break game functionality!");
        
        Helpers.PatchMethod(
            Helpers.TypeFromQualifiedName("Robust.Client.Console.ClientConsoleHost"),
            "RemoteExecuteCommand",
            typeof(Blackhole),
            "RECPref",
            HarmonyPatchType.Prefix
            );
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