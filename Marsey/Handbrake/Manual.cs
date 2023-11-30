using System.Reflection;
using HarmonyLib;

namespace Marsey.Handbrake;

/// <summary>
/// In some cases we are required to patch functions publicly
/// </summary>
public static class Manual
{
    public static void Prefix(MethodInfo method, MethodInfo prefix)
    {
        Harmony? harm = HarmonyManager.GetHarmony();
        harm?.Patch(method, prefix: prefix);
    }

    public static void Postfix(MethodInfo method, MethodInfo postfix)
    {
        Harmony? harm = HarmonyManager.GetHarmony();
        harm?.Patch(method, postfix: postfix);
    }

    public static void Transpiler(MethodInfo method, MethodInfo transpiler)
    {
        Harmony? harm = HarmonyManager.GetHarmony();
        harm?.Patch(method, transpiler: transpiler);
    }

    /// <summary>
    /// Removes **all** patches of type from a method
    /// </summary>
    /// <param name="method">target method that has patches applies</param>
    /// <param name="patchType">type of patches</param>
    public static void Unpatch(MethodInfo method, HarmonyPatchType patchType)
    {
        Harmony? harm = HarmonyManager.GetHarmony();
        harm?.Unpatch(method, patchType);
    }
}