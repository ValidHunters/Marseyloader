using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using Marsey.Config;
using Marsey.Handbreak;
using Marsey.Misc;
using Marsey.Stealthsey;

namespace Marsey.Game.Patches;

/// <summary>
/// Manages HWId variable given to the game.
/// </summary>
public static class HWID
{
    private static byte[] _hwId = Array.Empty<byte>();
    private static string _hwidString = "";

    /// <summary>
    /// Patching the HWId function and replacing it with a custom HWId.
    /// </summary>
    public static void Force()
    {
        // TODO: Further work on HWID2
        // While currently trust is not used, I suspect having an empty hwid string will harm its value
        // This is made so you couldn't hwid ban other people this easily
        // As of 10/4/2024 hwid2 instead of this bullshit you can pass an env variable to tell the client that you're not sending your hwid

        // Check if forcing is enabled
        if (!MarseyConf.ForceHWID)
            return;

        MarseyLogger.Log(MarseyLogger.LogType.INFO, "HWIDForcer", $"Trying to apply {_hwidString}");

        string cleanedHwid = CleanHwid(_hwidString);
        ForceHWID(cleanedHwid);
        PatchCalcMethod();
    }

    public static void SetHWID(string hwid)
    {
        _hwidString = hwid;
    }

    private static string CleanHwid(string hwid)
    {
        return new string(hwid.Where(c => "0123456789ABCDEF".Contains(c)).ToArray());
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

    public static string GenerateRandom(int length = 64)
    {
        Random random = new Random();
        const string chars = "0123456789ABCDEF";
        StringBuilder result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }

        return result.ToString();
    }

    private static void PatchCalcMethod()
    {
        Type? hwid1 = Helpers.TypeFromQualifiedName("Robust.Shared.Network.HWId");
        Type? hwid2 = Helpers.TypeFromQualifiedName("Robust.Client.Hwid.BasicHwid");

        if (hwid1 is not null)
        {
            Helpers.PatchMethod(
                hwid1,
                "Calc",
                typeof(HWID),
                "RecalcHwid",
                HarmonyPatchType.Postfix
            );
            return;
        }

        if (hwid2 is not null)
        {
            Helpers.PatchMethod(
                hwid2,
                "GetLegacy",
                typeof(HWID),
                nameof(RecalcHwid),
                HarmonyPatchType.Postfix
            );

            Helpers.PatchMethod(
                hwid2,
                "GetModern",
                typeof(HWID),
                nameof(RecalcHwid2),
                HarmonyPatchType.Postfix
            );
        }
    }

    public static bool CheckHWID(string hwid)
    {
        return Regex.IsMatch(hwid, "^$|^[A-F0-9]{64}$");
    }

    private static void RecalcHwid(ref byte[] __result)
    {
        string hwidString = BitConverter.ToString(_hwId).Replace("-", "");
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "HWIDForcer", $"\"Recalculating\" HWID to {hwidString}");
        __result = _hwId;
    }

    private static void RecalcHwid2(ref byte[] __result)
    {
        string hwidString = BitConverter.ToString(_hwId).Replace("-", "");
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "HWIDForcer", $"\"Recalculating\" HWID to {hwidString}");
        __result = [0, .._hwId];
    }
}
