using System.Reflection;

namespace Marsey.Stealthsey;

public static class HideseyPatches
{
    /// <summary>
    /// This is a postfix patch which swaps an assembly list with a less honest one
    /// </summary>
    /// <returns></returns>
    public static void LieLoader(ref Assembly[] __result)
    {
        __result = Hidesey.LyingDomain(__result);
    }

    /// <summary>
    /// This patch skips function execution
    /// </summary>
    public static bool Skip() => false;
}