using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Marsey.Handbreak;
using Marsey.Stealthsey;
using Marsey.Stealthsey.Reflection;

namespace Marsey.Subversion;

/// <summary>
/// Manages the subversion hide queue, hides subversions
/// </summary>
public static class Sedition
{
    private static List<Assembly> _queue = new List<Assembly>();

    [HideLevelRequirement(HideLevel.Normal)]
    public static void Patch()
    {
        MethodInfo? Target = Helpers.GetMethod("Content.Client.Entry.EntryPoint", "SwitchToDefaultState");
        MethodInfo? Patch = Helpers.GetMethod(typeof(Sedition), "Postfix");
        Manual.Patch(Target, Patch, HarmonyPatchType.Postfix);
    }

    public static void Queue(Assembly subversion)
    {
        _queue.Add(subversion);
    }

    [UsedImplicitly]
    private static void Postfix()
    {
        foreach (Assembly asm in _queue)
        {
            Hidesey.HidePatch(asm);
        }
    }
}
