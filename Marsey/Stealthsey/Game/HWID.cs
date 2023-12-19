using System;
using System.Reflection;
using HarmonyLib;
using Marsey.Handbrake;
using Marsey.Misc;
using Marsey.PatchAssembly;
using Marsey.Stealthsey.Reflection;

namespace Marsey.Stealthsey.Game;

public static class HWID
{
    private static byte[] HWId = Array.Empty<byte>();
    
    [HideLevelRequirement(HideLevel.Normal)]
    public static void Force()
    {
        string hwid = GetForcedHWId();
        
        // Remove any non-hexadecimal characters
        string cleanedHwid = new string(hwid.Where(c => "0123456789ABCDEFabcdef".Contains(c)).ToArray());
           
        MarseyLogger.Log(MarseyLogger.LogType.INFO, "HWIDForcer", "Priming");
        // Assuming HWId is a byte array representing hardware ID
        // Convert each pair of characters to a byte
        HWId = new byte[cleanedHwid.Length / 2];
        try
        {
            for (int i = 0; i < cleanedHwid.Length; i += 2)
            {
                string byteValue = cleanedHwid.Substring(i, 2);
                HWId[i / 2] = Convert.ToByte(byteValue, 16); // Convert hex string to byte
            }
        }
        catch (FormatException ex)
        {
            MarseyLogger.Log(MarseyLogger.LogType.INFO, $"Invalid HWID format, must be hexadecimal: {ex.Message}.\nSetting to null.");
            HWId = Array.Empty<byte>();
        }
        
        
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "HWIDForcer", "Starting capture");
        // Capture the calc function
        Assembly? RobustAssembly = AssemblyFieldHandler.GetGameAssemblies()[1]; // Robust.Shared
        if (RobustAssembly == null) return;
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "HWIDForcer", "Type");
        Type? HWIdType = RobustAssembly.GetType("Robust.Shared.Network.HWId");
        if (HWIdType == null) return;
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "HWIDForcer", "Method");
        MethodInfo? CalcMethod = HWIdType.GetMethod("Calc", BindingFlags.Public | BindingFlags.Static);
        if (CalcMethod == null) return;
        
        // Get recalc and patch
        MethodInfo RecalcMethod =
            typeof(HWID).GetMethod("RecalcHwid", BindingFlags.Static | BindingFlags.NonPublic)!;
        Manual.Patch(CalcMethod, RecalcMethod, HarmonyPatchType.Postfix);
    }

    private static string HWIDEnv = "MARSEY_FORCEDHWID";
    private static string GetForcedHWId()
    {
        string? hwid = Environment.GetEnvironmentVariable(HWIDEnv);
        
        Envsey.CleanFlag(HWIDEnv);
        
        if (hwid == null)
            return String.Empty;

        return hwid;
    }
    
    private static void RecalcHwid(ref byte[] __result)
    {
        // Convert the HWId byte array to a hexadecimal string
        string hwidString = BitConverter.ToString(HWId).Replace("-", "");
    
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "HWIDForcer", $"\"Recalculating\" HWID to {hwidString}");
        __result = HWId;
    }

}