using System.Reflection;
using HarmonyLib;
using Marsey.Config;
using Marsey.Game.Managers;
using Marsey.Misc;

namespace Marsey.Handbreak;

/// <summary>
/// In some cases we are required to patch functions from the comfort of the loader
/// </summary>
public static class Manual
{
    public static bool Patch(MethodInfo? method, MethodInfo? patch, HarmonyPatchType type)
    {
        try
        {
            if (method == null || patch == null)
                throw new HandBreakException($"Attempted to patch {method} with {patch}, but one of them is null!");

            switch (type)
            {
                case HarmonyPatchType.Prefix:
                    Prefix(method, patch);
                    break;
                case HarmonyPatchType.Postfix:
                    Postfix(method, patch);
                    break;
                case HarmonyPatchType.Transpiler:
                    Transpiler(method, patch);
                    break;
                case HarmonyPatchType.Finalizer:
                    Finalizer(method, patch);
                    break;
                case HarmonyPatchType.ReversePatch:
                    Reverse(method, patch);
                    break;
                case HarmonyPatchType.All:
                default:
                    MarseyLogger.Log(MarseyLogger.LogType.ERRO, $"Passed an invalid patchtype: {type}");
                    break;
            }
        }
        catch (Exception e)
        {
            string message = $"Encountered an issue with patching {method?.Name} against {patch?.Name}!\n{e}";
            MarseyLogger.Log(MarseyLogger.LogType.ERRO, "HandBreak", message);
            
            if (MarseyConf.ThrowOnFail)
                throw new HandBreakException(message);
            
            return false;
        }

        return true;
    }
    
    private static void Prefix(MethodBase method, MethodInfo prefix)
    {
        Harmony harm = HarmonyManager.GetHarmony();
        harm.Patch(method, prefix: prefix);
    }

    private static void Postfix(MethodBase method, MethodInfo postfix)
    {
        Harmony harm = HarmonyManager.GetHarmony();
        harm.Patch(method, postfix: postfix);
    }

    private static void Transpiler(MethodBase method, MethodInfo transpiler)
    {
        Harmony harm = HarmonyManager.GetHarmony();
        harm.Patch(method, transpiler: transpiler);
    }

    private static void Finalizer(MethodBase method, MethodInfo finalizer)
    {
        Harmony harm = HarmonyManager.GetHarmony();
        harm.Patch(method, finalizer: finalizer);
    }
    
    private static void Reverse(MethodBase method, MethodInfo reversepatch)
    {
        Harmony harm = HarmonyManager.GetHarmony();
        ReversePatcher rPatcher = harm.CreateReversePatcher(method, reversepatch);
        rPatcher.Patch();
    }

    /// <summary>
    /// Removes all patches of type from a method
    /// </summary>
    /// <param name="method">target method that has patches applies</param>
    /// <param name="patchType">type of patches</param>
    public static void Unpatch(MethodInfo method, HarmonyPatchType patchType)
    {
        Harmony harm = HarmonyManager.GetHarmony();
        harm.Unpatch(method, patchType);
    }
}