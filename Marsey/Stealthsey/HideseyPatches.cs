using System.Reflection;

namespace Marsey.Stealthsey;

/// <summary>
/// Manual patches used with Hidesey
/// Not based off MarseyPatch or SubverterPatch
/// </summary>
public static class HideseyPatches
{
    /// <summary>
    /// This is a postfix patch which swaps an assembly list with a less honest one
    /// </summary>
    public static void LieLoader(ref Assembly[] __result)
    {
        __result = Hidesey.LyingDomain(__result);
    }

    public static void LieReference(ref AssemblyName[] __result)
    {
        __result = Hidesey.LyingReference(__result);
    }

    /// <summary>
    /// This patch skips function execution
    /// </summary>
    public static bool Skip() => false;
}