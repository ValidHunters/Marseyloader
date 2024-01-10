using System;
using System.Text;
using System.Reflection;
using HarmonyLib;
using Marsey.Config;
using Marsey.Handbreak;
using Marsey.Misc;
using Marsey.PatchAssembly;
using Marsey.Stealthsey;
using Marsey.Stealthsey.Reflection;

namespace Marsey.GameAssembly.Patches;

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
        
        MarseyLogger.Log(MarseyLogger.LogType.INFO, "HWIDForcer", "Starting");

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
    
    public static string GenerateRandom(int length)
    {
        Random random = new Random();
        const string chars = "0123456789abcdef";
        StringBuilder result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }

        return result.ToString();
    }

    private static void PatchCalcMethod()
    {
        Helpers.PatchMethod(
            Helpers.TypeFromQualifiedName("Robust.Shared.Network.HWId"),
            "Calc",
            typeof(HWID),
            "RecalcHwid",
            HarmonyPatchType.Postfix
            );
    }

    private static void RecalcHwid(ref byte[] __result)
    {
        string hwidString = BitConverter.ToString(_hwId).Replace("-", "");
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "HWIDForcer", $"\"Recalculating\" HWID to {hwidString}");
        __result = _hwId;
    }
}
