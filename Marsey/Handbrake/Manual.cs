using System.Reflection;
using HarmonyLib;
using Marsey.GameAssembly;
using Marsey.Misc;

namespace Marsey.Handbrake;

/// <summary>
/// In some cases we are required to patch functions from the comfort of the loader
/// </summary>
public static class Manual
{
    public static void Patch(MethodInfo? method, MethodInfo? patch, HarmonyPatchType type)
    {
        if (method == null || patch == null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.FATL, $"HardPatch failed! Tried to patch method {method?.Name} with {patch?.Name} ({type.ToString()}).");
            return;
        }

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
                Reverse(method,patch);
                break;
            case HarmonyPatchType.All:
            default:
                MarseyLogger.Log(MarseyLogger.LogType.FATL, $"Passed an invalid patchtype: {type.ToString()}");
                break;
        }
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