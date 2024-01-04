using System;
using System.Reflection;
using HarmonyLib;
using Marsey.Config;
using Marsey.GameAssembly;
using Marsey.Handbrake;
using Marsey.Misc;
using Marsey.PatchAssembly;
using Marsey.Stealthsey.Reflection;

namespace Marsey.Stealthsey.Game;

/// <summary>
/// Manages HWId variable given to the game.
/// </summary>
public static class HWID
{
    private static byte[] _hwId = Array.Empty<byte>();
    private const string HWIDEnv = "MARSEY_FORCEDHWID";

    /// <summary>
    /// Patching the HWId function and replacing it with a custom HWId.
    /// </summary>
    /// <remarks>Requires Normal or above MarseyHide</remarks>
    public static void Force()
    {
        /// Only accepts a hexidecimal string, so you don't get to write "FUCK YOU PJB/SLOTH/RANE/MOONY/FAYE/SMUG/EXEC/ALLAH/EMO/ONIKS/MORTY".
        /// Maybe if you wrote it in entirely numeric, with "Rane" being 18F1F14F5 or something.
        /// Nobody will read that anyway - its for ban evasion and thats it.
        /// Don't forget a VPN or a proxy!
        
        // Check if forcing is enabled
        if (!MarseyConf.ForceHWID)
            return;

        string hwid = GetForcedHWId();
        string cleanedHwid = CleanHwid(hwid);
        ForceHWID(cleanedHwid);
        PatchCalcMethod();
    }

    private static string GetForcedHWId()
    {
        string? hwid = Environment.GetEnvironmentVariable(HWIDEnv);
        Envsey.CleanFlag(HWIDEnv);
        return hwid ?? string.Empty;
    }

    private static string CleanHwid(string hwid)
    {
        return new string(hwid.Where(c => "0123456789ABCDEFabcdef".Contains(c)).ToArray());
    }

    private static void ForceHWID(string cleanedHwid)
    {
        MarseyLogger.Log(MarseyLogger.LogType.INFO, "HWIDForcer", "Priming");
        try
        {
            _hwId = Enumerable.Range(0, cleanedHwid.Length / 2)
                              .Select(x => Convert.ToByte(cleanedHwid.Substring(x * 2, 2), 16))
                              .ToArray();
        }
        catch (FormatException ex)
        {
            MarseyLogger.Log(MarseyLogger.LogType.INFO, $"Invalid HWID format, must be hexadecimal: {ex.Message}.\nSetting to null.");
            _hwId = Array.Empty<byte>();
        }
    }

    private static void PatchCalcMethod()
    {
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "HWIDForcer", "Starting capture");
        Assembly? robustAssembly = GameAssemblies.RobustShared;
        if (robustAssembly == null) return;

        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "HWIDForcer", "Type");
        Type? hwIdType = robustAssembly.GetType("Robust.Shared.Network.HWId");
        if (hwIdType == null) return;

        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "HWIDForcer", "Method");
        MethodInfo? calcMethod = hwIdType.GetMethod("Calc", BindingFlags.Public | BindingFlags.Static);
        if (calcMethod == null) return;

        MethodInfo recalcMethod = typeof(HWID).GetMethod("RecalcHwid", BindingFlags.Static | BindingFlags.NonPublic)!;
        Manual.Patch(calcMethod, recalcMethod, HarmonyPatchType.Postfix);
    }

    private static void RecalcHwid(ref byte[] __result)
    {
        string hwidString = BitConverter.ToString(_hwId).Replace("-", "");
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "HWIDForcer", $"\"Recalculating\" HWID to {hwidString}");
        __result = _hwId;
    }
}
