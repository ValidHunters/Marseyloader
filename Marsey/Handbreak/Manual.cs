using System.Reflection;
using HarmonyLib;

namespace Marsey.Handbreak;

/// <summary>
/// In some cases we are required to patch functions publicly
/// </summary>
public static class Manual
{
    public static void ManualPrefix(MethodInfo method, MethodInfo prefix)
    {
        Harmony? harm = HarmonyManager.GetHarmony();
        harm?.Patch(method, prefix: prefix);
    }

    public static void ManualPostfix(MethodInfo method, MethodInfo postfix)
    {
        Harmony? harm = HarmonyManager.GetHarmony();
        harm?.Patch(method, postfix: postfix);
    }

    public static void ManualTranspiler(MethodInfo method, MethodInfo transpiler)
    {
        Harmony? harm = HarmonyManager.GetHarmony();
        harm?.Patch(method, transpiler: transpiler);
    }
}